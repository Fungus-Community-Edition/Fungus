using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityObj = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.VScripting
{
    /// <summary>
    /// Centralized registry that keeps Flowcharts discoverable in both the editor and at runtime.
    /// </summary>
    public static class FlowchartRegistry
    {
        static FlowchartRegistry()
        {
            EnsureInitialized();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod()]
        private static void OnEditorLoad()
        {
            Debug.Log("FlowchartRegistry initializing on editor load.");
            EnsureInitialized(true);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeLoad()
        {
            // Note: RuntimeInitializeLoadType.BeforeSceneLoad makes this execute once per app launch,
            // right before the first scene is loaded. Not right before just any scene is loaded.
            EnsureInitialized(true);
        }

        public static void EnsureInitialized(bool forceReinitialize = false)
        {
            if (isInitialized && !forceReinitialize)
            {
                return;
            }

            ToggleSubs(false);
            ToggleSubs(true);
            CaptureExistingFlowcharts();
            isInitialized = true;
        }

        private static bool isInitialized;

        private static void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartSignals.FlowchartEnabled += RegisterFlowchart;
                FlowchartSignals.FlowchartDestroyed += UnregisterFlowchart;
            }
            else
            {
                FlowchartSignals.FlowchartEnabled -= RegisterFlowchart;
                FlowchartSignals.FlowchartDestroyed -= UnregisterFlowchart;
            }
            
        }

        private static void CaptureExistingFlowcharts()
        {
            flowchartLookup.Clear();
            Flowchart[] existingFlowcharts = UnityObj.FindObjectsByType<Flowchart>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < existingFlowcharts.Length; i++)
            {
                RegisterFlowchart(existingFlowcharts[i]);
            }
        }

        private static void RegisterFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
                Debug.Log($"Registering Flowchart {flowchart.name} into registry");
                flowchartLookup[flowchart.UniqueId] = flowchart;
            }
        }

        private static readonly object syncLock = new object();
        private static readonly Dictionary<string, Flowchart> flowchartLookup =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        private static void UnregisterFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
                Debug.Log($"Unregistering Flowchart {flowchart.name} from registry");
                flowchartLookup.Remove(flowchart.UniqueId);
            }
        }

        public static IReadOnlyList<Flowchart> GetFlowcharts()
        {
            lock (syncLock)
            {
                return flowchartLookup.Values.ToList();
            }
        }

        public static Flowchart GetFChartWith(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return null;
            }

            lock (syncLock)
            {
                flowchartLookup.TryGetValue(guid, out Flowchart flowchart);
                return flowchart;
            }
        }

        public static void Clear()
        {
            lock (syncLock)
            {
                flowchartLookup.Clear();
            }
        }
    }
}