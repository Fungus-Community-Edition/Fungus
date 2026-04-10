using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AtMycelia.SaveSys;
using AtMycelia.SaveSys.VScripting;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.SaveSys
{
    public class SaveLoadedBlockExecutor
    {
        public void OnEnable()
        {
            FlowchartApplier.AppliedToAllFcs += OnAppliedToAllFlowcharts;
        }

        private void OnAppliedToAllFlowcharts()
        {
            ExecuteSaveLoadedBlocks();
        }

        private void ExecuteSaveLoadedBlocks()
        {
            IList<SaveLoadedEvent> handlers = UnityObj.FindObjectsByType<SaveLoadedEvent>(FindObjectsSortMode.None).ToList();
            if (handlers.Count == 0)
            {
                return;
            }

            HashSet<string> registeredMarkers = SaveSystem.ProgressMarkers
                .Select(marker => marker.Id)
                .ToHashSet();

            handlers = handlers
                .OrderBy(handler => handler.LowestOrder())
                .ThenBy(handler => handler.GetInstanceID())
                .ToList();

            for (int i = 0; i < handlers.Count; i++)
            {
                var handler = handlers[i];
                if (handler == null || !handler.gameObject.activeInHierarchy)
                {
                    continue;
                }

                bool hasMatchingMarker = handler.MarkerIDs.Any(id => registeredMarkers.Contains(id));

                bool shouldRespond = handler.RespondToAny || hasMatchingMarker;
                if (!shouldRespond)
                {
                    continue;
                }

                Debug.Log($"Invoking SaveLoadedEvent handler on Block {handler.ParentBlock.BlockName} on Flowchart " +
                    $"{handler.gameObject.name} with marker IDs {string.Join(", ", handler.MarkerIDs)}");
                handler.ExecuteBlock();
            }
        }

        public void OnDisable()
        {
            FlowchartApplier.AppliedToAllFcs -= OnAppliedToAllFlowcharts;
        }


    }
}