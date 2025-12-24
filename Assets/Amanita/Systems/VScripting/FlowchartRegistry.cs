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
            EnsureInitialized();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnRuntimeLoad()
        {
            EnsureInitialized();
        }

        private static void EnsureInitialized()
        {
            if (isInitialized)
            {
                return;
            }

            SubscribeToSignals();
            CaptureExistingFlowcharts();
            isInitialized = true;
        }

        private static bool isInitialized;

        private static void SubscribeToSignals()
        {
            FlowchartSignals.FlowchartEnabled -= RegisterFlowchart;
            FlowchartSignals.FlowchartEnabled += RegisterFlowchart;

            FlowchartSignals.FlowchartDestroyed -= UnregisterFlowchart;
            FlowchartSignals.FlowchartDestroyed += UnregisterFlowchart;
        }

        private static void CaptureExistingFlowcharts()
        {
            Flowchart[] existingFlowcharts = UnityObj.FindObjectsByType<Flowchart>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < existingFlowcharts.Length; i++)
            {
                RegisterFlowchart(existingFlowcharts[i]);
            }
        }

        public static void RegisterFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
                flowchartLookup[flowchart.UniqueId] = flowchart;
            }
        }

        private static readonly object syncLock = new object();
        private static readonly Dictionary<string, Flowchart> flowchartLookup =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        public static void UnregisterFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
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