using System;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Coordinates selection changes in the flowchart window, ensuring that the correct flowchart is selected
    /// and displayed when the selection changes.
    /// </summary>
    internal sealed class FcwSelectionCoordinator
    {
        public void HandleSelectionChanged(Flowchart previous, Flowchart current,
            ref FlowchartContext context, UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel)
        {
            context ??= new FlowchartContext();

            Flowchart resolved;
            if (current != null && ReferenceEquals(current, previous))
            {
                resolved = current;
            }
            else
            {
                resolved = EditorSelectionTracker.LastActiveFlowchart;
            }

            context.Flowchart = resolved;

            UpdateLabels(context, fcNameLabel, zoomAmountLabel);
            if (resolved == null || ReferenceEquals(previous, resolved))
            {
                return;
            }

            if (previous != null)
            {
                previous.ClearSelectedBlocks();
                previous.ClearSelectedCommands();
            }

            FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
        }

        public void UpdateLabels(FlowchartContext context, UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel)
        {
            if (context == null || context.Flowchart == null)
            {
                fcNameLabel.text = zoomAmountLabel.text = string.Empty;
                return;
            }

            if (fcNameLabel != null)
            {
                fcNameLabel.text = $"FC: {context.Flowchart.name}";
            }

            if (zoomAmountLabel != null)
            {
                zoomAmountLabel.text = $"Zoom: {Math.Round(context.Flowchart.Zoom * 100)}%";
            }
        }

        public void UpdateZoom(UitkLabel zoomAmountLabel, float newZoom)
        {
            if (zoomAmountLabel == null)
            {
                return;
            }

            zoomAmountLabel.text = $"Zoom: {Math.Round(newZoom * 100)}%";
        }
    }
}