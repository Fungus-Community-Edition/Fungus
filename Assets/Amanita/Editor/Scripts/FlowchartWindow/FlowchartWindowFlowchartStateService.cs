using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.VScripting.EditorUtils.FcWindow
{
    public sealed class FlowchartWindowFlowchartStateService
    {
        public Flowchart ResolveSelectionChange(Flowchart previous, Flowchart current)
        {
            if (current == null && previous != null)
            {
                return previous;
            }

            if (current != null)
            {
                return current;
            }

            return FindFirstObjectByType<Flowchart>();
        }

        private T FindFirstObjectByType<T>() where T : UnityObj
        {
            return UnityObj.FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
        }

        public void ResetSelections(Flowchart flowchart)
        {
            if (flowchart == null)
            {
                return;
            }

            flowchart.ClearSelectedBlocks();
            flowchart.ClearSelectedCommands();
        }

        public Flowchart ResolveFlowchartForScene(Flowchart currentFlowchart, Flowchart lastFocusedInPlayMode, 
            out bool usedPlayModeFlowchart)
        {
            usedPlayModeFlowchart = lastFocusedInPlayMode != null;
            if (usedPlayModeFlowchart)
            {
                return lastFocusedInPlayMode;
            }

            if (currentFlowchart != null)
            {
                return currentFlowchart;
            }

            return FindFirstObjectByType<Flowchart>();
        }

        public bool TryGetFlowchartByUid(string flowchartUid, out Flowchart flowchart)
        {
            flowchart = null;
            if (string.IsNullOrEmpty(flowchartUid))
            {
                return false;
            }

            Flowchart[] fcsInScene = UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            flowchart = fcsInScene.FirstOrDefault(fc => fc.UniqueId == flowchartUid);
            return flowchart != null;
        }

        public Flowchart ResolveRefreshFlowchart(Flowchart activeFlowchart)
        {
            if (activeFlowchart != null)
            {
                return activeFlowchart;
            }

            return FindFirstObjectByType<Flowchart>();
        }
    }
}