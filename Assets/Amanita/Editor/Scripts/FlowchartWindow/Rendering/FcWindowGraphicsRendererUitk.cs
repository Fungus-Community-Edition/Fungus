using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils.FcWindow
{
    /// <summary>
    /// Encapsulates all flowchart window graphics renderers (grid, blocks, selection box).
    /// </summary>
    public sealed class FcWindowGraphicsRendererUitk : VisualElement, IFlowchartWindowModule, IDisposable,
        IFlowchartChangeResponder, IScrollWheelMoveResponder, IWindowPanResponder, IBlockCreatedResponder,
        IBlockSelectionResponder, IMultiBlockSelectionResponder, IBlockDeselectionResponder, IMultiBlockDeselectionResponder,
        IPreBlockDeletionResponder, IPostBlockDeletionResponder, IPostMultiBlockDeletionResponder,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IPreBlockCutResponder, IPostBlockCutResponder, IPreMultiBlockCutResponder, IPostMultiBlockCutResponder
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
            _repaintTriggerer = new FcWindowRepaintTriggerer();
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

            #region Add visual elements
            Add(gridRenderer);
            Add(blockRenderer);
            Add(connectionRenderer);
            Add(selectionBoxRenderer);
            #endregion

            #region Register Submodules
            _submodules.Add(gridRenderer);
            _submodules.Add(blockRenderer);
            _submodules.Add(connectionRenderer);
            _submodules.Add(selectionBoxRenderer);
            _submodules.Add(_repaintTriggerer);
            #endregion
        }

        private readonly GridRendererUitk gridRenderer;
        private readonly BlockRendererUitk blockRenderer;
        private readonly SelectionBoxRendererUitk selectionBoxRenderer;
        private readonly ConnectionRendererUitk connectionRenderer;
        private FcWindowRepaintTriggerer _repaintTriggerer;
        private bool isDisposed;

        private readonly IList<IFlowchartWindowModule> _submodules = new List<IFlowchartWindowModule>();
        // ^ Cache of all submodules for easy iteration in event handlers.
        public IReadOnlyList<IFlowchartWindowModule> Submodules => (IReadOnlyList<IFlowchartWindowModule>)_submodules;
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
            blockRenderer.RefreshBlocks();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            for (int i = 0; i < _submodules.Count; i++)
            {
                _submodules[i].Dispose();
            }
            RemoveFromHierarchy();
        }

        public void OnScrollWheelMoved()
        {
            Forward<IScrollWheelMoveResponder>(r => r.OnScrollWheelMoved());
        }

        private void Forward<TResponder>(Action<TResponder> action)
            where TResponder : class
        {
            for (int i = 0; i < _submodules.Count; i++)
            {
                if (_submodules[i] is not TResponder responder)
                {
                    continue;
                }

                action(responder);
            }
        }

        public void OnWindowPanned()
        {
            Forward<IWindowPanResponder>(r => r.OnWindowPanned());
        }

        public void OnBlockSelected(Block block)
        {
            Forward<IBlockSelectionResponder>(r => r.OnBlockSelected(block));
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            Forward<IMultiBlockSelectionResponder>(r => r.OnMultiBlocksSelected(blocks));
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            Forward<IFlowchartChangeResponder>(r => r.OnFlowchartChanged(previous, next));
        }

        public void OnPreBlockDeletion(IList<Block> blocks)
        {
            Forward<IPreBlockDeletionResponder>(r => r.OnPreBlockDeletion(blocks));
        }

        public void OnPreBlockDeletion(Block block)
        {
            Forward<IPreBlockDeletionResponder>(r => r.OnPreBlockDeletion(block));
        }

        public void OnPostBlockDeletion(ushort blockId)
        {
            Forward<IPostBlockDeletionResponder>(r => r.OnPostBlockDeletion(blockId));
        }

        public void OnPostMultiBlockDeletion(IList<ushort> blockIds)
        {
            Forward<IPostMultiBlockDeletionResponder>(r => r.OnPostMultiBlockDeletion(blockIds));
        }

        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragStartResponder>(r => r.OnLeftMouseDragStarted(info, evt));
        }

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragResponder>(r => r.OnLeftMouseDragged(info, evt));
        }

        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt)
        {
            Forward<ILeftMouseDragEndResponder>(r => r.OnLeftMouseDragEnded(info, evt));
        }

        public void OnBlockDeselected(Block block)
        {
            Forward<IBlockDeselectionResponder>(r => r.OnBlockDeselected(block));
        }

        public void OnMultiBlocksDeselected(IList<Block> blocks)
        {
            Forward<IMultiBlockDeselectionResponder>(r => r.OnMultiBlocksDeselected(blocks));
        }

        public void OnBlockCreated(Block block)
        {
            Forward<IBlockCreatedResponder>(r => r.OnBlockCreated(block));
        }

        public void OnPreBlockCut(Block block)
        {
            Forward<IPreBlockCutResponder>(r => r.OnPreBlockCut(block));
        }

        public void OnPostBlockCut(ushort blockId)
        {
            Forward<IPostBlockCutResponder>(r => r.OnPostBlockCut(blockId));
        }

        public void OnPreMultiBlockCut(IList<Block> blocks)
        {
            Forward<IPreMultiBlockCutResponder>(r => r.OnPreMultiBlockCut(blocks));
        }

        public void OnPostMultiBlockCut(IList<ushort> blockIds)
        {
            Forward<IPostMultiBlockCutResponder>(r => r.OnPostMultiBlockCut(blockIds));
        }
    }
}