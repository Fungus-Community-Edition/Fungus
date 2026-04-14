using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using AtMycelia.Graphics;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public interface IBlockDrawerUitk
    {
        BlockButton CreateButton(Block block);
        void UpdateButton(BlockButton button, Block block, float zoom);
    }

    /// <summary>
    /// Renders Flowchart blocks as UITK buttons that size themselves to their contents.
    /// </summary>
    public sealed class BlockRenderer : VisualElement, IFlowchartWindowModule, IDisposable,
        IFlowchartChangeResponder, IWindowPanResponder, IScrollWheelMoveResponder,
        IBlockCreatedResponder,
        IBlockSelectionResponder, IPreBlockDeletionResponder,
        IPostBlockDeletionResponder, IPostMultiBlockDeletionResponder,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder,
        ILeftMouseDragEndResponder, IBlockDeselectionResponder, IMultiBlockSelectionResponder,
        IMultiBlockDeselectionResponder, IBlockRectProvider,
        IPostBlockCutResponder, IPostMultiBlockCutResponder, IVisualResetter
    {
        public int Priority { get; set; } = 0;
        private readonly Dictionary<Block, BlockBinding> blockBindings = new();
        private FlowchartWindow owner;
        private bool isDisposed;
        private bool initialRefreshPending;

        /// <summary>
        /// Binds a block to its visual representation and event handlers.
        /// </summary>
        private sealed class BlockBinding
        {
            public BlockButton Button;
            public Action ClickHandler;
        }
        
        public BlockRenderer(FlowchartContext context, IBlockDrawerUitk blockDrawer)
        {
            fcContext = context ?? throw new ArgumentNullException(nameof(context));
            drawer = blockDrawer ?? throw new ArgumentNullException(nameof(blockDrawer));

            style.position = Position.Absolute;
            style.flexGrow = 1f;

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                Undo.undoRedoPerformed += OnUndoRedoPerformedFirst;
            }
            else
            {
                Undo.undoRedoPerformed -= OnUndoRedoPerformedFirst;
            }
        }

        private void OnUndoRedoPerformedFirst()
        {
            ClearAll(); // Helps prevent some buttons from sticking around when they shouldn't.
            RefreshBlocks();
        }

        private readonly FlowchartContext fcContext;
        private readonly IBlockDrawerUitk drawer;

        public void Initialize(FlowchartWindow window)
        {
            owner = window;
            initialRefreshPending = true;
            ToggleSubs(false);
            ToggleSubs(true);
            TryRefreshAfterLayout();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            TryRefreshAfterLayout();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!initialRefreshPending)
            {
                return;
            }

            if (evt.newRect.width <= 0f || evt.newRect.height <= 0f)
            {
                return;
            }

            TryRefreshAfterLayout();
        }

        private void TryRefreshAfterLayout()
        {
            if (!initialRefreshPending)
            {
                return;
            }

            if (panel == null)
            {
                return;
            }

            if (contentRect.width <= 0f || contentRect.height <= 0f)
            {
                return;
            }

            initialRefreshPending = false;
            RefreshBlocks();
        }

        public void RefreshBlocks()
        {
            if (isDisposed)
            {
                return;
            }

            Flowchart flowchart = fcContext.Flowchart;
            if (flowchart == null)
            {
                ClearAll();
                return;
            }

            IReadOnlyCollection<Block> present = fcContext.Document.AllBlocks;
            RemoveMissing(present);

            foreach (var block in present)
            {
                EnsureBlockVisual(block);
            }

            UpdateBlockLayouts();
            FlowchartWindowSignals.WindowPanned();
            MarkDirtyRepaint();
        }

        private void RemoveMissing(IReadOnlyCollection<Block> currentBlocks)
        {
            using ListPool<Block>.DisposableList pooledKeysHandle = ListPool<Block>.Get(out List<Block> pooledKeys);
            pooledKeys.AddRange(blockBindings.Keys);
            for (int i = 0; i < pooledKeys.Count; i++)
            {
                Block tracked = pooledKeys[i];
                if (!ContainsBlock(currentBlocks, tracked))
                {
                    RemoveBlock(tracked);
                }
            }
        }

        private static bool ContainsBlock(IReadOnlyCollection<Block> blocks, Block target)
        {
            if (blocks == null)
            {
                return false;
            }

            if (blocks is ICollection<Block> collection)
            {
                return collection.Contains(target);
            }

            foreach (var block in blocks)
            {
                if (ReferenceEquals(block, target))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveBlock(Block block)
        {
            if (!blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                return;
            }

            BlockButton buttonToRemove = binding.Button;
            if (buttonToRemove != null)
            {
                UnregisterInputForwarders(buttonToRemove);

                if (binding.ClickHandler != null)
                {
                    buttonToRemove.Clicked -= binding.ClickHandler;
                }

                buttonToRemove.Dispose();
            }

            blockBindings.Remove(block);
            MarkDirtyRepaint();
        }

        private void EnsureBlockVisual(Block block)
        {
            if (block == null)
            {
                return;
            }

            bool blockAlreadyDrawn = blockBindings.TryGetValue(block, out BlockBinding binding);
            if (!blockAlreadyDrawn)
            {
                BlockButton button = drawer.CreateButton(block);
                button.name = block.BlockName;
                button.style.position = Position.Absolute;

                RegisterInputForwarders(button);

                var capturedBlock = block;
                button.Clicked += OnClick;
                void OnClick()
                {
                    BlockSignals.BlockLeftClicked?.Invoke(capturedBlock, Event.current);
                }
                
                void OnButtonGeometryChanged(GeometryChangedEvent evt)
                {
                    if (evt.newRect.width <= 0f || evt.newRect.height <= 0f)
                    {
                        return;
                    }
                    // We do this (calling UpdateButton on the first geometry change) so that right when the
                    // window opens, the button is rendered at the right size. For some reason, putting
                    // RefreshBlocks in Initialize doesn't work...
                    button.UnregisterCallback<GeometryChangedEvent>(OnButtonGeometryChanged);
                    drawer.UpdateButton(button, capturedBlock, CurrentZoom);
                    UpdateBlockLayouts();
                }
                button.RegisterCallback<GeometryChangedEvent>(OnButtonGeometryChanged);

                ScheduleInitialRefresh(button, capturedBlock);

                binding = new BlockBinding
                {
                    Button = button,
                    ClickHandler = OnClick
                };
                
                blockBindings.Add(block, binding);
                Add(button);
            }

            drawer.UpdateButton(binding.Button, block, CurrentZoom);
        }

        private void ScheduleInitialRefresh(BlockButton button, Block block)
        {
            if (button == null || block == null)
            {
                return;
            }

            button.schedule.Execute(() =>
            {
                if (button.panel == null)
                {
                    return;
                }

                if (!blockBindings.TryGetValue(block, out BlockBinding binding) || binding.Button != button)
                {
                    return;
                }

                drawer.UpdateButton(button, block, CurrentZoom);
                UpdateBlockLayouts();
            }).ExecuteLater(1);
        }

        /// <summary>
        /// Based on the current scroll and zoom, update the positions and sizes of all block buttons.
        /// </summary>
        private void UpdateBlockLayouts()
        {
            Vector2 scroll = CurrentScroll;
            float zoom = CurrentZoom;

            foreach (var pair in blockBindings)
            {
                Block block = pair.Key;
                BlockButton button = pair.Value.Button;
                if (block == null || button == null)
                {
                    continue;
                }

                Rect rect = block._NodeRect;
                Vector2 viewPos = (rect.position + scroll) * zoom;

                button.style.left = viewPos.x;
                button.style.top = viewPos.y;

                drawer.UpdateButton(button, block, zoom);
            }
        }

        private Vector2 CurrentScroll
        {
            get
            {
                Flowchart flowchart = fcContext.Flowchart;
                return flowchart != null ? flowchart.ScrollPos : Vector2.zero;
            }
        }

        private float CurrentZoom
        {
            get
            {
                Flowchart flowchart = fcContext.Flowchart;
                float zoom = flowchart != null ? flowchart.Zoom : 1f;
                return Mathf.Approximately(zoom, 0f) ? 1f : zoom;
            }
        }

        #region Callbacks
        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            ClearAll();
            initialRefreshPending = true;
            TryRefreshAfterLayout();
            SchedulePostLayoutRefresh();
        }

        public void OnWindowPanned()
        {
            UpdateBlockLayouts();
        }

        public void OnScrollWheelMoved()
        {
            UpdateBlockLayouts();
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            UpdateButtonForMultiBlocks(blocks);
        }

        private void UpdateButtonForMultiBlocks(IList<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                UpdateButtonForBlock(blocks[i]);
            }
        }

        private void UpdateButtonForBlock(Block block)
        {
            // It's possible that this is being called in response to a block from another
            // Flowchart being deselected due to a Flowchart change. In that case, we won't
            // have a binding for this block, and that's fine - we just won't update any button.
            if (block == null)
            {
                return;
            }
            if (blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                drawer.UpdateButton(binding.Button, block, CurrentZoom);
            }
            else
            {
                // We probably just created this block, so ensure it has a visual.
                EnsureBlockVisual(block);
            }
        }

        public void OnBlockDeselected(Block block)
        {
            UpdateButtonForBlock(block);
        }

        public void OnMultiBlocksDeselected(IList<Block> blocks)
        {
            UpdateButtonForMultiBlocks(blocks);
        }

        #endregion

        public void OnBlockSelected(Block block)
        {
            UpdateButtonForBlock(block);
        }

        private void ClearAll()
        {
            foreach (var entry in blockBindings)
            {
                UnregisterInputForwarders(entry.Value.Button);
                UnsubClickHandler(entry.Value);
                entry.Value.Button?.Dispose();
            }
            blockBindings.Clear();
        }

        private void UnsubClickHandler(BlockBinding binding)
        {
            if (binding.Button != null && binding.ClickHandler != null)
            {
                binding.Button.Clicked -= binding.ClickHandler;
            }
        }

        public void OnPreBlockDeletion(IList<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var blockEl = blocks[i];
                RemoveBlock(blockEl);
            }
        }

        public void OnPreBlockDeletion(Block block)
        {
            RemoveBlock(block);
        }

        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt)
        {
            #region Keep Blocks from blocking drag events
            foreach (var entry in blockBindings)
            {
                var button = entry.Value.Button;
                if (button != null)
                {
                    button.SetPickingMode(PickingMode.Ignore);
                }
            }
            #endregion
        }

        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt)
        {
            #region Let Blocks be selectable again
            foreach (var entry in blockBindings)
            {
                var button = entry.Value.Button;
                if (button != null)
                {
                    button.SetPickingMode(PickingMode.Position);
                }
            }
            #endregion
        }

        public bool TryGetBlockRect(Block block, out Rect rect)
        {
            rect = default;
            if (block == null)
            {
                return false;
            }

            if (!blockBindings.TryGetValue(block, out BlockBinding binding) || binding.Button == null)
            {
                return false;
            }

            VisualElement parentEl = parent;
            Rect worldRect = binding.Button.worldBound;
            if (IsInvalidRect(worldRect))
            {
                return false;
            }

            if (parentEl == null)
            {
                rect = worldRect;
                return true;
            }

            Vector2 localPos = parentEl.WorldToLocal(worldRect.position);
            rect = new Rect(localPos, worldRect.size);
            return true;
        }

        private static bool IsInvalidRect(Rect rect)
        {
            return IsInvalidNumber(rect.x) ||
                   IsInvalidNumber(rect.y) ||
                   IsInvalidNumber(rect.width) ||
                   IsInvalidNumber(rect.height) ||
                   rect.width <= 0f ||
                   rect.height <= 0f;
        }

        private static bool IsInvalidNumber(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            ToggleSubs(false);
            isDisposed = true;
            ClearAll();
            UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RemoveFromHierarchy();
        }

        private InputSignalModule InputSignals => owner != null ? owner.InputSignals : null;

        private void RegisterInputForwarders(BlockButton button)
        {
            VisualElement inputTarget = button != null ? button.InputTarget : null;
            if (inputTarget == null)
            {
                return;
            }

            inputTarget.RegisterCallback<PointerDownEvent>(OnBlockPointerDown);
            inputTarget.RegisterCallback<PointerMoveEvent>(OnBlockPointerMove);
            inputTarget.RegisterCallback<PointerUpEvent>(OnBlockPointerUp);
            inputTarget.RegisterCallback<PointerCancelEvent>(OnBlockPointerCancel);
        }

        private void UnregisterInputForwarders(BlockButton button)
        {
            VisualElement inputTarget = button != null ? 
                button.InputTarget : 
                null;
            if (inputTarget == null)
            {
                return;
            }

            inputTarget.UnregisterCallback<PointerDownEvent>(OnBlockPointerDown);
            inputTarget.UnregisterCallback<PointerMoveEvent>(OnBlockPointerMove);
            inputTarget.UnregisterCallback<PointerUpEvent>(OnBlockPointerUp);
            inputTarget.UnregisterCallback<PointerCancelEvent>(OnBlockPointerCancel);
        }

        private void OnBlockPointerDown(PointerDownEvent evt)
        {
            InputSignals?.OnPointerDown(evt);
        }

        private void OnBlockPointerMove(PointerMoveEvent evt)
        {
            InputSignals?.OnPointerMove(evt);
        }

        private void OnBlockPointerUp(PointerUpEvent evt)
        {
            //Debug.Log("BlockRendererUitk received pointer up event, forwarding to InputSignals.");
            //InputSignals?.OnPointerUp(evt);
        }

        private void OnBlockPointerCancel(PointerCancelEvent evt)
        {
            // No op
        }

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            if (fcContext.Interaction.BlockDragOngoing)
            {
                UpdateBlockLayouts();
            }
        }

        public void OnBlockCreated(Block block)
        {
            UpdateButtonForBlock(block);
        }

        public void OnPostBlockDeletion(ushort blockId)
        {
            // Why do this in post? It's because by the time that the pre signal fires, the
            // block(s) are still registered in the Flowchart. That leads to the
            // should've-been-deleted blocks still being drawn in RefreshBlocks, which causes
            // weird visual bugs. By waiting until post, we ensure that the blocks are fully
            // deleted from the Flowchart before we try to refresh our visuals.
            ClearAll();
            RefreshBlocks();
        }

        public void OnPostMultiBlockDeletion(IList<ushort> blockIds)
        {
            ClearAll();
            RefreshBlocks();
        }

        public void OnPostBlockCut(ushort blockId)
        {
            OnPostBlockDeletion(blockId);
        }

        public void OnPostMultiBlockCut(IList<ushort> blockIds)
        {
            OnPostMultiBlockDeletion(blockIds);
        }

        public void ResetVisuals()
        {
            if (isDisposed)
            {
                return;
            }

            ClearAll();
            initialRefreshPending = true;

            if (panel != null && contentRect.width > 0f && contentRect.height > 0f)
            {
                initialRefreshPending = false;
                RefreshBlocks();
                SchedulePostLayoutRefresh();
                return;
            }

            schedule.Execute(TryRefreshAfterLayout).ExecuteLater(1);
            SchedulePostLayoutRefresh();
        }

        private void SchedulePostLayoutRefresh()
        {
            schedule.Execute(() =>
            {
                if (isDisposed)
                {
                    return;
                }

                RefreshBlocks();
            }).ExecuteLater(1);
        }
    }

}