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
        IFlowchartChangeResponder, IScrollWheelMoveResponder, IWindowPanResponder, 
        IBlockSelectionResponder, IMultiBlockSelectionResponder, IBlockDeselectionResponder, IMultiBlockDeselectionResponder,
        IPreBlockDeletionResponder, ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder
    {
        public int Priority { get; set; } = 0;
        public FcWindowGraphicsRendererUitk(FlowchartContext context, DrawGridContext gridDrawContext,
            IBlockDrawerUitk blockDrawer)
        {
            #region Validate Parameters
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
            #endregion

            #region Create Submodules
            gridRenderer = new GridRendererUitk(context, gridDrawContext);
            blockRenderer = new BlockRendererUitk(context, blockDrawer);
            selectionBoxRenderer = new SelectionBoxRendererUitk(context);
            var connectionDrawer = new ConnectionDrawerUitk(new ConnectionGathererUitk(blockRenderer));
            connectionRenderer = new ConnectionRendererUitk(context, connectionDrawer);
            #endregion

            #region Position and Style
            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.left = 0f;
            // ^ All set to 0 so this can work with the entire window, not just some part of it.
            // The window's padding will create the necessary offset from the edges.
            style.flexGrow = 1f;
            #endregion

            Add(gridRenderer);
            Add(blockRenderer);
            Add(connectionRenderer);
            Add(selectionBoxRenderer);

            submodules.Add(gridRenderer);
            submodules.Add(blockRenderer);
            submodules.Add(connectionRenderer);
            submodules.Add(selectionBoxRenderer);
        }

        private readonly GridRendererUitk gridRenderer;
        private readonly BlockRendererUitk blockRenderer;
        private readonly SelectionBoxRendererUitk selectionBoxRenderer;
        private readonly ConnectionRendererUitk connectionRenderer;
        private bool isDisposed;

        private readonly IList<IFlowchartWindowModule> submodules = new List<IFlowchartWindowModule>();
        // ^ Cache of all submodules for easy iteration in event handlers.

        public void Initialize(FlowchartWindowUitk window)
        {
            gridRenderer.Initialize(window);
            connectionRenderer.Initialize(window);
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
            for (int i = 0; i < submodules.Count; i++)
            {
                submodules[i].Dispose();
            }
            RemoveFromHierarchy();
        }

        public void OnScrollWheelMoved()
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IScrollWheelMoveResponder responder)
                {
                    continue;
                }
                responder.OnScrollWheelMoved();
            }
        }

        public void OnWindowPanned()
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IWindowPanResponder responder)
                {
                    continue;
                }
                responder.OnWindowPanned();
            }
        }

        public void OnBlockSelected(Block block)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IBlockSelectionResponder responder)
                {
                    continue;
                }
                responder.OnBlockSelected(block);
            }
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IMultiBlockSelectionResponder responder)
                {
                    continue;
                }
                responder.OnMultiBlocksSelected(blocks);
            }
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IFlowchartChangeResponder responder)
                {
                    continue;
                }
                responder.OnFlowchartChanged(previous, next);
            }
        }

        public void OnPreBlockDeletion(IList<Block> blocks)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IPreBlockDeletionResponder responder)
                {
                    continue;
                }
                responder.OnPreBlockDeletion(blocks);
            }
        }

        public void OnPreBlockDeletion(Block block)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IPreBlockDeletionResponder responder)
                {
                    continue;
                }
                responder.OnPreBlockDeletion(block);
            }
        }

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not ILeftMouseDragStartResponder responder)
                {
                    continue;
                }
                responder.OnLeftMouseDragStarted(startPos, evt);
            }
        }

        public void OnLeftMouseDragged(Vector2 delta, Event evt)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not ILeftMouseDragResponder responder)
                {
                    continue;
                }
                responder.OnLeftMouseDragged(delta, evt);
            }
        }

        public void OnLeftMouseDragEnded(Vector2 endPos, Event evt)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not ILeftMouseDragEndResponder responder)
                {
                    continue;
                }
                responder.OnLeftMouseDragEnded(endPos, evt);
            }
        }

        public void OnBlockDeselected(Block block)
        {
             for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IBlockDeselectionResponder responder)
                {
                    continue;
                }
                responder.OnBlockDeselected(block);
            }
        }

        public void OnMultiBlocksDeselected(IList<Block> blocks)
        {
            for (int i = 0; i < submodules.Count; i++)
            {
                var sModule = submodules[i];
                if (sModule is not IMultiBlockDeselectionResponder responder)
                {
                    continue;
                }
                responder.OnMultiBlocksDeselected(blocks);
            }
        }
    }
}