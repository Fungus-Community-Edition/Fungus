using System;
using Amanita.VScripting;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// To keep the FlowchartWindow and BlockInspector synced with the last Flowchart selected.
    /// </summary>
    public sealed class FlowchartSelectionSyncerUitk : IFlowchartWindowModule,
        IFlowchartChangeResponder, IBlockSelectionResponder
    {
        public FlowchartSelectionSyncerUitk(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        private readonly FlowchartContext flowchartContext;

        public void Initialize(FlowchartWindowUitk window)
        {
            isDisposed = false; // We may want to re-initialize after dispose.
            owner = window != null ? 
                window : 
                throw new ArgumentNullException(nameof(window));
            SetAsTrackedFlowchart(flowchartContext.Flowchart);
            ToggleSubs(true);
        }

        private bool isDisposed;
        private FlowchartWindowUitk owner;

        private void SetAsTrackedFlowchart(Flowchart next)
        {
            bool alreadyTrackingIt = ReferenceEquals(trackedFlowchart, next);
            if (alreadyTrackingIt)
            {
                return;
            }

            trackedFlowchart = next;
            lastSelectedBlock = trackedFlowchart != null ?
                trackedFlowchart.SelectedBlock :
                null;
        }

        private Flowchart trackedFlowchart;
        private Block lastSelectedBlock;

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                BlockSignals.BlockCreated += OnBlockCreated;
            }
            else
            {
                BlockSignals.BlockCreated -= OnBlockCreated;
            }
        }

        private void OnBlockCreated(Block block)
        {
            if (!isDisposed)
            {
                SetFlowchartAsSelecting(block);
            }
        }

        private void SetFlowchartAsSelecting(Block block)
        {
            Flowchart flowchart = Flowchart;
            bool validFlowchart = flowchart != null;
            if (!validFlowchart)
            {
                return;
            }

            bool validBlock = block != null;
            if (!validBlock)
            {
                flowchart.ClearSelectedBlocks();
                flowchart.ClearSelectedCommands();
                lastSelectedBlock = null;
                return;
            }

            bool alreadySelected = ReferenceEquals(flowchart.SelectedBlock, block);
            if (alreadySelected)
            {
                return;
            }

            flowchart.ClearSelectedCommands(); // For all we know, the commands could belong to another block
            flowchart.SelectedBlock = block;
            lastSelectedBlock = block;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            ToggleSubs(false);
            SetAsTrackedFlowchart(null);
            owner = null;
            trackedFlowchart = null;
            lastSelectedBlock = null;
        }

        public void OnBlockSelected(Block block)
        {
            if (!isDisposed)
            {
                SetFlowchartAsSelecting(block);
            }
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            if (isDisposed)
            {
                return;
            }

            SetAsTrackedFlowchart(next);

            if (next == null)
            {
                SetFlowchartAsSelecting(null);
                return;
            }

            SetFlowchartAsSelecting(next.SelectedBlock);
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;
    }
}