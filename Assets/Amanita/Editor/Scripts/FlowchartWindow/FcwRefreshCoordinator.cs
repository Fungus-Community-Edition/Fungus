using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    internal sealed class FcwRefreshCoordinator
    {
        private readonly FcwFlowchartStateService _flowchartStateService;

        public FcwRefreshCoordinator(FcwFlowchartStateService flowchartStateService)
        {
            _flowchartStateService = flowchartStateService;
        }

        public void HandleRefresh(
            Func<Flowchart> activeFlowchartGetter,
            MissingFlowchartOverlay missingOverlay,
            Action rebuildGui)
        {
            Flowchart activeFlowchart = activeFlowchartGetter != null ?
                activeFlowchartGetter() :
                null;

            Flowchart flowchart = _flowchartStateService.ResolveRefreshFlowchart(activeFlowchart);
            if (activeFlowchart != null && flowchart != null)
            {
                Selection.activeGameObject = flowchart.gameObject;
            }

            if (flowchart)
            {
                Debug.Log("Flowchart found on refresh.");
                missingOverlay?.Hide();
                rebuildGui?.Invoke();
            }
            else
            {
                Debug.LogWarning("Flowchart still not found on refresh.");
            }
        }
    }
}