using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Encapsulates all flowchart window graphics renderers (grid, blocks, selection box).
    /// </summary>
    public sealed class FcWindowGraphicsRendererUitk : VisualElement, IFlowchartWindowModule, IDisposable,
        IScrollWheelMoveResponder, IWindowPanResponder, IBlockSelectionResponder, IFlowchartChangeResponder,
        IPreBlockDeletionResponder, ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder
    {
        public FcWindowGraphicsRendererUitk(FlowchartContext context, DrawGridContext gridDrawContext,
            IBlockDrawerUitk blockDrawer)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (gridDrawContext == null)
            {
                throw new ArgumentNullException(nameof(gridDrawContext));
            }

            if (blockDrawer == null)
            {
                throw new ArgumentNullException(nameof(blockDrawer));
            }

            gridRenderer = new GridRendererUitk(context, gridDrawContext);
            blockRenderer = new BlockRendererUitk(context, blockDrawer);
            selectionBoxRenderer = new SelectionBoxRendererUitk(context);

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.left = 0f;
            style.flexGrow = 1f;

            Add(gridRenderer);
            Add(blockRenderer);
            Add(selectionBoxRenderer);
        }

        private readonly GridRendererUitk gridRenderer;
        private readonly BlockRendererUitk blockRenderer;
        private readonly SelectionBoxRendererUitk selectionBoxRenderer;
        private bool isDisposed;

        public void Initialize(FlowchartWindowUitk window)
        {
            gridRenderer.Initialize(window);
            blockRenderer.Initialize(window);
            selectionBoxRenderer.Initialize(window);
        }

        public void RefreshNow()
        {
            gridRenderer.RefreshNow();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            gridRenderer.Dispose();
            blockRenderer.Dispose();
            selectionBoxRenderer.Dispose();
            RemoveFromHierarchy();
        }

        public void OnScrollWheelMoved()
        {
            gridRenderer.OnScrollWheelMoved();
            blockRenderer.OnScrollWheelMoved();
            selectionBoxRenderer.OnScrollWheelMoved();
        }

        public void OnWindowPanned()
        {
            gridRenderer.OnWindowPanned();
            blockRenderer.OnWindowPanned();
            selectionBoxRenderer.OnWindowPanned();
        }

        public void OnBlockSelected(Block block)
        {
            gridRenderer.OnBlockSelected(block);
            blockRenderer.OnBlockSelected(block);
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            gridRenderer.OnMultiBlocksSelected(blocks);
            blockRenderer.OnMultiBlocksSelected(blocks);
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            gridRenderer.OnFlowchartChanged(previous, next);
            blockRenderer.OnFlowchartChanged(previous, next);
            selectionBoxRenderer.OnFlowchartChanged(previous, next);
        }

        public void OnPreBlockDeletion(IList<Block> blocks)
        {
            blockRenderer.OnPreBlockDeletion(blocks);
        }

        public void OnPreBlockDeletion(Block block)
        {
            blockRenderer.OnPreBlockDeletion(block);
        }

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            blockRenderer.OnLeftMouseDragStarted(startPos, evt);
            selectionBoxRenderer.OnLeftMouseDragStarted(startPos, evt);
        }

        public void OnLeftMouseDragged(Vector2 delta, Event evt)
        {
            selectionBoxRenderer.OnLeftMouseDragged(delta, evt);
        }

        public void OnLeftMouseDragEnded(Vector2 endPos, Event evt)
        {
            blockRenderer.OnLeftMouseDragEnded(endPos, evt);
            selectionBoxRenderer.OnLeftMouseDragEnded(endPos, evt);
        }
    }
}