using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using System;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FcwSelectionCoordinator
    {
        private readonly FcwFlowchartStateService _flowchartStateService;
        private readonly FcwPlayModeFocusService _playModeFocusService;

        public FcwSelectionCoordinator(
            FcwFlowchartStateService flowchartStateService,
            FcwPlayModeFocusService playModeFocusService)
        {
            _flowchartStateService = flowchartStateService;
            _playModeFocusService = playModeFocusService;
        }

        public void HandleSelectionChanged(
            Flowchart previous,
            Flowchart current,
            FlowchartContext context,
            UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel)
        {
            if (context == null)
            {
                return;
            }

            Flowchart resolved = _flowchartStateService.ResolveSelectionChange(previous, current);
            if (ReferenceEquals(previous, resolved))
            {
                return;
            }

            if (previous != null)
            {
                _flowchartStateService.ResetSelections(previous);
            }

            context.Flowchart = resolved;

            string cachedUid;
            if (_playModeFocusService.TryCacheFromSelection(resolved, out cachedUid))
            {
                Debug.Log($"In Play Mode - updated last-focused flowchart UID to {cachedUid}");
            }

            UpdateLabels(context, fcNameLabel, zoomAmountLabel);
            FlowchartWindowSignals.ChangedFlowchart(previous, resolved);
        }

        public void UpdateLabels(
            FlowchartContext context,
            UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel)
        {
            if (context == null || context.Flowchart == null)
            {
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