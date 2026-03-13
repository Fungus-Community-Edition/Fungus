using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FlowchartWindowPlayModeCoordinator
    {
        private readonly FlowchartWindowPlayModeFocusService _playModeFocusService;
        private readonly FlowchartWindowFlowchartStateService _flowchartStateService;
        private readonly FlowchartWindowSelectionCoordinator _selectionCoordinator;

        public FlowchartWindowPlayModeCoordinator(
            FlowchartWindowPlayModeFocusService playModeFocusService,
            FlowchartWindowFlowchartStateService flowchartStateService,
            FlowchartWindowSelectionCoordinator selectionCoordinator)
        {
            _playModeFocusService = playModeFocusService;
            _flowchartStateService = flowchartStateService;
            _selectionCoordinator = selectionCoordinator;
        }

        public void HandlePlayModeStateChanged(
            PlayModeStateChange state,
            Func<Flowchart> activeFlowchartGetter,
            FlowchartContext context,
            UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel,
            FcWindowGraphicsRenderer graphicsRenderer)
        {
            if (state != PlayModeStateChange.EnteredEditMode &&
                state != PlayModeStateChange.EnteredPlayMode &&
                state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                string cachedUid;
                Flowchart activeFlowchart = activeFlowchartGetter != null ?
                    activeFlowchartGetter() :
                    null;

                if (_playModeFocusService.TryCacheFromActiveFlowchart(activeFlowchart, out cachedUid))
                {
                    Debug.Log($"Entered play mode - cached last-focused flowchart UID as {cachedUid}");
                }
            }

            if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += () =>
                {
                    if (context == null)
                    {
                        return;
                    }

                    Flowchart lastFocusedInPlayMode;
                    if (_playModeFocusService.TryResolveLastFocused(_flowchartStateService, out lastFocusedInPlayMode))
                    {
                        Debug.Log($"Found last-focused flowchart from play mode on exit: {lastFocusedInPlayMode.name}");
                        Selection.activeGameObject = lastFocusedInPlayMode.gameObject;
                        context.Flowchart = lastFocusedInPlayMode;
                        _selectionCoordinator.UpdateLabels(context, fcNameLabel, zoomAmountLabel);
                    }
                    else if (_playModeFocusService.HasCachedFocus)
                    {
                        Debug.LogWarning("Could not find last-focused flowchart from play mode on exit.");
                    }

                    graphicsRenderer?.ResetVisuals();
                };
            }
        }
    }
}