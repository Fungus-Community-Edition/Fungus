using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// For handling what happens when the Flowchart Window's Refresh button is clicked. 
    /// This is necessary to coordinate the various components of the window, such as
    /// updating the active flowchart, hiding the missing flowchart overlay, and rebuilding the GUI.
    /// </summary>
    internal sealed class FcwRefreshCoordinator
    {
        public void HandleRefresh(Func<Flowchart> activeFlowchartGetter, MissingFlowchartOverlay missingOverlay,
            Action rebuildGui)
        {
            Flowchart flowchart = activeFlowchartGetter != null ?
                activeFlowchartGetter() :
                null;

            if (flowchart == null)
            {
                flowchart = EditorSelectionTracker.LastActiveFlowchart;
            }

            if (flowchart != null)
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