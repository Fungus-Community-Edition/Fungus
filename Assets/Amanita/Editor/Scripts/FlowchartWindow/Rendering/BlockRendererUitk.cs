using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkButton = UnityEngine.UIElements.Button;

namespace Amanita.VScripting.EditorUtils
{
    public interface IBlockDrawerUitk
    {
        UitkButton CreateButton(Block block);
        void UpdateButton(UitkButton button, Block block, float zoom);
    }

    /// <summary>
    /// Renders Flowchart blocks as UITK buttons that size themselves to their contents.
    /// </summary>
    internal sealed class BlockRendererUitk : VisualElement, IFlowchartWindowModule, IDisposable,
        IFlowchartChangeResponder, IWindowPanResponder, IScrollWheelMoveResponder,
        IBlockSelectionResponder, IPreBlockDeletionResponder, ILeftMouseDragStartResponder,
        ILeftMouseDragEndResponder, IBlockDeselectionResponder, IMultiBlockSelectionResponder,
        IMultiBlockDeselectionResponder, IBlockRectProvider
    {
        private readonly Dictionary<Block, BlockBinding> blockBindings = new();
        private FlowchartWindowUitk owner;
        private bool isDisposed;

        /// <summary>
        /// Binds a block to its visual representation and event handlers.
        /// </summary>
        private sealed class BlockBinding
        {
            public UitkButton Button;
            public Action ClickHandler;
        }
        
        public BlockRendererUitk(FlowchartContext context, IBlockDrawerUitk blockDrawer)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            drawer = blockDrawer ?? throw new ArgumentNullException(nameof(blockDrawer));

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.flexGrow = 1f;
        }

        private readonly FlowchartContext flowchartContext;
        private readonly IBlockDrawerUitk drawer;

        public void Initialize(FlowchartWindowUitk window)
        {
            owner = window;
            EditorApplication.delayCall += () => RefreshBlocks(); 
            // ^To make sure the blocks render at the right size on initial window open. Otherwise,
            // they render at the wrong size until selected.
        }

        public void RefreshBlocks()
        {
            if (isDisposed)
            {
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                ClearAll();
                return;
            }

            IReadOnlyCollection<Block> present = flowchartContext.Document.AllBlocks;
            RemoveMissing(present);

            foreach (var block in present)
            {
                EnsureBlockVisual(block);
            }

            UpdateBlockLayouts();
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

            if (binding.Button != null)
            {
                if (binding.ClickHandler != null)
                {
                    binding.Button.clicked -= binding.ClickHandler;
                }
                binding.Button.RemoveFromHierarchy();
            }

            blockBindings.Remove(block);
        }

        private void EnsureBlockVisual(Block block)
        {
            if (block == null)
            {
                return;
            }

            if (!blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                UitkButton button = drawer.CreateButton(block);
                button.style.position = Position.Absolute;

                var capturedBlock = block;
                void OnClick()
                {
                    BlockSignals.BlockClicked?.Invoke(capturedBlock, Event.current);
                }
                button.clicked += OnClick;

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
                UitkButton button = pair.Value.Button;
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
                Flowchart flowchart = flowchartContext.Flowchart;
                return flowchart != null ? flowchart.ScrollPos : Vector2.zero;
            }
        }

        private float CurrentZoom
        {
            get
            {
                Flowchart flowchart = flowchartContext.Flowchart;
                float zoom = flowchart != null ? flowchart.Zoom : 1f;
                return Mathf.Approximately(zoom, 0f) ? 1f : zoom;
            }
        }

        #region Callbacks
        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            RefreshBlocks();
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
            if (blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                drawer.UpdateButton(binding.Button, block, CurrentZoom);
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
                UnsubClickHandler(entry.Value);
                entry.Value.Button?.RemoveFromHierarchy();
            }
            blockBindings.Clear();
        }

        private void UnsubClickHandler(BlockBinding binding)
        {
            if (binding.Button != null && binding.ClickHandler != null)
            {
                binding.Button.clicked -= binding.ClickHandler;
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

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            #region Keep Blocks from blocking drag events
            foreach (var entry in blockBindings)
            {
                var button = entry.Value.Button;
                if (button != null)
                {
                    button.pickingMode = PickingMode.Ignore;
                }
            }
            #endregion
        }

        public void OnLeftMouseDragEnded(Vector2 endPos, Event evt)
        {
            #region Let Blocks be selectable again
            foreach (var entry in blockBindings)
            {
                var button = entry.Value.Button;
                if (button != null)
                {
                    button.pickingMode = PickingMode.Position;
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

            if (parentEl == null)
            {
                rect = worldRect;
                return true;
            }

            Vector2 localPos = parentEl.WorldToLocal(worldRect.position);
            rect = new Rect(localPos, worldRect.size);
            return true;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            ClearAll();
            this.RemoveFromHierarchy();
        }
    }

}