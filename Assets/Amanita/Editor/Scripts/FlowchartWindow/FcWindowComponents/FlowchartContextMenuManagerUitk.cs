using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Shows the empty space popup window on right-click and listens for its button actions.
    /// </summary>
    public sealed class FlowchartContextMenuManagerUitk : IFlowchartWindowModule, 
        IRightClickResponder, ILeftMouseUpResponder
    {
        public int Priority { get; set; } = 0;

        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            owner = window;
            isDisposed = false;

            EnsurePopupsExist();
            ToggleSubs(true);
        }

        private FlowchartWindowUitk owner;
        private bool isDisposed;

        private void EnsurePopupsExist()
        {
            if (_emptySpacePopup != null && _blockPopup != null)
            {
                return;
            }

            _emptySpacePopup = new FcEmptySpacePopupWindow();
            _blockPopup = new BlockContextMenuUitk();
            _emptySpacePopup.style.position = _blockPopup.style.position = Position.Absolute;
            // ^So the popups can be positioned relative to the mouse click position without being affected by layout.
        }

        private FcEmptySpacePopupWindow _emptySpacePopup;
        private BlockContextMenuUitk _blockPopup;

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartWindowSignals.EmptySpaceRightClicked += OnEmptySpaceRightClicked;
                BlockSignals.BlockLeftClicked += OnBlockLeftClicked;
                BlockSignals.BlockRightClicked += OnBlockRightClicked;
            }
            else
            {
                FlowchartWindowSignals.EmptySpaceRightClicked -= OnEmptySpaceRightClicked;
                BlockSignals.BlockLeftClicked -= OnBlockLeftClicked;
                BlockSignals.BlockRightClicked -= OnBlockRightClicked;
            }

            if (owner != null)
            {
                if (on)
                {
                    owner.rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
                }
                else
                {
                    owner.rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
                }
            }

            if (_emptySpacePopup == null)
            {
                return;
            }

            if (on)
            {
                _emptySpacePopup.AddButtonClicked += OnAddButtonClicked;
                _emptySpacePopup.PasteButtonClicked += OnPasteButtonClicked;
            }
            else
            {
                _emptySpacePopup.AddButtonClicked -= OnAddButtonClicked;
                _emptySpacePopup.PasteButtonClicked -= OnPasteButtonClicked;
            }
        }

        private void OnEmptySpaceRightClicked(PointerEventInfo info)
        {
            if (isDisposed || owner == null)
            {
                return;
            }
            bool anyBlocksSelected = owner.FcContext.Selection.BlockCount > 0;
            if (anyBlocksSelected)
            {
                return;
            }

            _lastRightClickInfo = info;
            EnsurePopupsExist();
            HideBlockPopup();
            UpdatePosCache();
            EnsureOnScreenAtFront(_emptySpacePopup);
            PositionRelativeToMouse(_emptySpacePopup);
        }

        private PointerEventInfo _lastRightClickInfo;

        private void HideBlockPopup()
        {
            _blockPopup?.RemoveFromHierarchy();
        }

        private void UpdatePosCache()
        {
            lastPopupFlowchartPosition = _lastRightClickInfo.FlowchartPosition;
            lastPopupPanelPosition = _lastRightClickInfo.PanelPosition;
            lastPopupWindowPosition = Root.WorldToLocal(_lastRightClickInfo.PanelPosition);
        }

        private Vector2? lastPopupFlowchartPosition;
        private Vector2? lastPopupPanelPosition;
        private Vector2? lastPopupWindowPosition;

        private void EnsureOnScreenAtFront(VisualElement popup)
        {
            if (popup.parent != Root)
            {
                popup.RemoveFromHierarchy();
                Root.Add(popup);
            }

            popup.BringToFront();
        }

        private VisualElement Root => owner.rootVisualElement;

        private void PositionRelativeToMouse(VisualElement popup)
        {
            Vector2 anchor = lastPopupWindowPosition ?? _lastRightClickInfo.PanelPosition;
            popup.style.left = anchor.x;
            popup.style.top = anchor.y;
        }

        private void OnBlockLeftClicked(Block block, Event @event)
        {
            if (isDisposed || owner == null)
            {
                return;
            }

            EnsurePopupsExist();
            HideAllPopups();
        }

        private void OnBlockRightClicked(Block block, PointerEventInfo info)
        {
            if (isDisposed || owner == null)
            {
                return;
            }

            EnsurePopupsExist();
            HideEmptySpacePopup();

            if (!block.IsSelected)
            {
                return;
            }

            _lastRightClickInfo = info;
            
            UpdatePosCache();
            EnsureOnScreenAtFront(_blockPopup);
            PositionRelativeToMouse(_blockPopup);
            _blockPopup.TargetBlock = block;
        }

        public void OnRightClick(PointerEventInfo info)
        {
            HideEmptySpacePopup();
        }

        private void HandleDismissClick(PointerEventInfo info)
        {
            if (isDisposed || !IsPopupVisible)
            {
                return;
            }

            HideAllPopups();
        }

        private void HideAllPopups()
        {
            HideEmptySpacePopup();
            HideBlockPopup();
        }

        public void OnLeftMouseUp(PointerEventInfo info, Event evt)
        {
            EditorApplication.delayCall += () => HandleDismissClick(info);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (isDisposed || !IsPopupVisible || evt == null)
            {
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                HideAllPopups();
            }
        }

        private bool IsPopupVisible => (_emptySpacePopup != null && _emptySpacePopup.parent != null) ||
            (_blockPopup != null && _blockPopup.parent != null);

        private void HideEmptySpacePopup()
        {
            _emptySpacePopup?.RemoveFromHierarchy();
        }

        private void OnAddButtonClicked()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("Empty space popup: Add button clicked.");
            FChart.ClearSelectedBlocks();
            Block newBlock = AddNewBlockToWindowAndFlowchart();
            Undo.RegisterCreatedObjectUndo(newBlock, "Add New Block");
            FChart.AddToSelection(newBlock);
            HideEmptySpacePopup();
        }

        private FlowchartContext FcContext => owner.FcContext;
        private Flowchart FChart => FcContext?.Flowchart;

        Block AddNewBlockToWindowAndFlowchart()
        {
            Vector2 blockLocation = DecideWhereToPlaceBlock();
            Vector2 DecideWhereToPlaceBlock()
            {
                Vector2 windowSpaceMousePos = lastPopupWindowPosition ?? Vector2.zero;
                float zoom = Mathf.Approximately(FChart.Zoom, 0f) ?
                    1f :
                    FChart.Zoom;
                Vector2 mousePosInFcSpace = (windowSpaceMousePos / zoom) - FChart.ScrollPos;
                mousePosInFcSpace -= offset;
                return mousePosInFcSpace;
            }

            var newBlock = FChart.CreateBlock(blockLocation);
            return newBlock;
        }

        private readonly Vector2 offset = new Vector2(70f, 15f);
        private void OnPasteButtonClicked()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("Empty space popup: Paste button clicked.");
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            ToggleSubs(false);

            _emptySpacePopup?.Dispose();
            _emptySpacePopup = null;
            
            _blockPopup?.Dispose();
            _blockPopup = null;

            owner = null;
        }

    }
}