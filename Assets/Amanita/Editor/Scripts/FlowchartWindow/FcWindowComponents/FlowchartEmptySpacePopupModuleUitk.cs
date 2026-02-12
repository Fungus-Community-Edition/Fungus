using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Shows the empty space popup window on right-click and listens for its button actions.
    /// </summary>
    public sealed class FlowchartEmptySpacePopupModuleUitk : IFlowchartWindowModule, 
        IRightClickResponder, ILeftMouseUpResponder
    {
        public int Priority { get; set; } = 0;

        private FlowchartWindowUitk owner;
        private FcEmptySpacePopupWindow popup;
        private bool isDisposed;

        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            owner = window;
            isDisposed = false;

            EnsurePopup();
            ToggleSubs(true);
        }

        private void EnsurePopup()
        {
            if (popup != null)
            {
                return;
            }

            popup = new FcEmptySpacePopupWindow();
            popup.style.position = Position.Absolute;
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartWindowSignals.EmptySpaceRightClicked += OnEmptySpaceRightClicked;
            }
            else
            {
                FlowchartWindowSignals.EmptySpaceRightClicked -= OnEmptySpaceRightClicked;
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

            if (popup == null)
            {
                return;
            }

            if (on)
            {
                popup.AddButtonClicked += OnAddButtonClicked;
                popup.PasteButtonClicked += OnPasteButtonClicked;
            }
            else
            {
                popup.AddButtonClicked -= OnAddButtonClicked;
                popup.PasteButtonClicked -= OnPasteButtonClicked;
            }
        }

        private Vector2? lastPopupFlowchartPosition;
        private Vector2? lastPopupPanelPosition;
        private Vector2? lastPopupWindowPosition;

        private void OnEmptySpaceRightClicked(PointerEventInfo info)
        {
            if (isDisposed || owner == null)
            {
                return;
            }

            EnsurePopup();
            _lastRightClickInfo = info;
            VisualElement root = owner.rootVisualElement;
            if (popup.parent != root)
            {
                popup.RemoveFromHierarchy();
                root.Add(popup);
            }

            lastPopupFlowchartPosition = _lastRightClickInfo.FlowchartPosition;
            lastPopupPanelPosition = _lastRightClickInfo.PanelPosition;
            lastPopupWindowPosition = root.WorldToLocal(_lastRightClickInfo.PanelPosition);

            PositionSelfRelativeToMouse();
            popup.BringToFront();
        }

        private PointerEventInfo _lastRightClickInfo;
        private void PositionSelfRelativeToMouse()
        {
            Vector2 anchor = lastPopupWindowPosition ?? _lastRightClickInfo.PanelPosition;
            popup.style.left = anchor.x;
            popup.style.top = anchor.y;
        }

        public void OnRightClick(PointerEventInfo info)
        {
            HandleDismissClick(info);
        }

        private void HandleDismissClick(PointerEventInfo info)
        {
            if (isDisposed || !IsPopupVisible)
            {
                return;
            }

            HidePopup();
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
                HidePopup();
            }
        }

        private bool IsPopupVisible => popup != null && popup.parent != null;

        private void HidePopup()
        {
            popup.RemoveFromHierarchy();
        }

        private void OnAddButtonClicked()
        {
            if (isDisposed)
            {
                return;
            }

            Debug.Log("Empty space popup: Add button clicked.");
            var fcContext = owner.FcContext;
            var fc = fcContext.Flowchart;

            Vector2 windowSpaceMousePos = lastPopupWindowPosition ?? Vector2.zero;
            float zoom = Mathf.Approximately(fc.Zoom, 0f) ? 
                1f : 
                fc.Zoom;
            Vector2 mousePosInFcSpace = (windowSpaceMousePos / zoom) - fc.ScrollPos;
            mousePosInFcSpace -= offset;

            fc.ClearSelectedBlocks();

            var newBlock = fc.CreateBlock(mousePosInFcSpace);

            Undo.RegisterCreatedObjectUndo(newBlock, "Add New Block");
            fc.AddToSelection(newBlock);
            HidePopup();
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

            if (popup != null)
            {
                popup.Dispose();
                popup = null;
            }

            owner = null;
        }

    }
}