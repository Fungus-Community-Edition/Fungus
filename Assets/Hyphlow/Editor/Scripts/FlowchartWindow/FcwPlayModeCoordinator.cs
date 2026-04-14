using System;
using UnityEditor;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Handles responses to changes in play mode state, ensuring that the flowchart
    /// window updates correctly when entering/exiting play mode or edit mode.
    /// </summary>
    internal sealed class FcwPlayModeCoordinator
    {
        public void HandlePlayModeStateChanged(PlayModeStateChange state, Func<Flowchart> activeFlowchartGetter,
            FlowchartContext context, UitkLabel fcNameLabel,
            UitkLabel zoomAmountLabel, FcwGraphicsRenderer graphicsRenderer)
        {
            if (state != PlayModeStateChange.EnteredEditMode &&
                state != PlayModeStateChange.EnteredPlayMode &&
                state != PlayModeStateChange.ExitingPlayMode)
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                if (context == null)
                {
                    return;
                }

                Flowchart activeFlowchart = EditorSelectionTracker.ResolveActiveFlowchart();
                if (activeFlowchart != null)
                {
                    Selection.activeGameObject = activeFlowchart.gameObject;
                    context.Flowchart = activeFlowchart;
                }

                graphicsRenderer?.ResetVisuals();
            };
            
        }
    }
}