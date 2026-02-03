using System;
using UnityEditor;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles Flowchart state changes that require the UITK window to refresh its data views.
    /// </summary>
    public sealed class FcWindowSelectionSyncUitk : IFlowchartWindowModule, IFlowchartChangeResponder
    {
        private readonly FlowchartContext flowchartContext;
        private FlowchartWindowUitk owner;
        private Flowchart trackedFlowchart;
        private bool skipNextEditorUpdate;
        private int prevVarCount;
        private bool isDisposed;

        public FcWindowSelectionSyncUitk(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            owner = window;
            isDisposed = false;

            SubscribeToFlowchart(flowchartContext.Flowchart);

            EditorApplication.update += OnEditorUpdate;
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            EditorApplication.update -= OnEditorUpdate;
            SubscribeToFlowchart(null);
            owner = null;
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            if (isDisposed)
            {
                return;
            }

            skipNextEditorUpdate = true;
            SubscribeToFlowchart(next);
        }

        private void OnEditorUpdate()
        {
            if (isDisposed || owner == null)
            {
                return;
            }

            Flowchart flowchart = Flowchart;
            if (flowchart == null)
            {
                owner.Repaint();
                return;
            }

            if (skipNextEditorUpdate)
            {
                skipNextEditorUpdate = false;
                return;
            }

            if (AnyNullBlocks())
            {
                owner.Repaint();
                return;
            }

            bool repaintRequested = false;

            if (flowchart.VariableCount != prevVarCount)
            {
                prevVarCount = flowchart.VariableCount;
                repaintRequested = true;
            }

            if (flowchart.SelectedCommandsStale)
            {
                flowchart.SelectedCommandsStale = false;
                repaintRequested = true;
            }

            if (CommandEditor.SelectedCommandDataStale)
            {
                CommandEditor.SelectedCommandDataStale = false;
                repaintRequested = true;
            }

            if (BlockEditor.SelectedBlockDataStale)
            {
                BlockEditor.SelectedBlockDataStale = false;
                repaintRequested = true;
            }

            if (FlowchartEditor.FlowchartDataStale)
            {
                FlowchartEditor.FlowchartDataStale = false;
                repaintRequested = true;
            }

            if (repaintRequested)
            {
                owner.Repaint();
            }
        }

        private void SubscribeToFlowchart(Flowchart next)
        {
            if (ReferenceEquals(trackedFlowchart, next))
            {
                return;
            }

            ToggleSubsOnTrackedFlowchart(false);
            trackedFlowchart = next;
            prevVarCount = trackedFlowchart != null ? trackedFlowchart.VariableCount : 0;
            ToggleSubsOnTrackedFlowchart(true);
        }

        private void ToggleSubsOnTrackedFlowchart(bool on)
        {
            if (trackedFlowchart == null)
            {
                return;
            }

            if (on)
            {
                trackedFlowchart.VariableAdded += OnVariableChanged;
                trackedFlowchart.VariableRemoved += OnVariableChanged;
            }
            else
            {
                trackedFlowchart.VariableAdded -= OnVariableChanged;
                trackedFlowchart.VariableRemoved -= OnVariableChanged;
            }
        }

        private void OnVariableChanged(IVariable _)
        {
            if (trackedFlowchart == null || owner == null)
            {
                return;
            }

            prevVarCount = trackedFlowchart.VariableCount;
            owner.Repaint();
        }

        private bool AnyNullBlocks()
        {
            var blocks = flowchartContext.Document?.AllBlocks;
            if (blocks == null)
            {
                return false;
            }

            foreach (var block in blocks)
            {
                if (block == null)
                {
                    return true;
                }
            }

            return false;
        }

        private Flowchart Flowchart => flowchartContext.Flowchart;
    }
}