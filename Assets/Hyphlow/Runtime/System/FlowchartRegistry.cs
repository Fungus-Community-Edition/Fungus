using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObj = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Centralized registry that keeps Flowcharts discoverable in both the editor and at runtime.
    /// </summary>
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class FlowchartRegistry
    {
        private static readonly bool _readAssetFlowchartsInRuntime = false;
        // ^ False by default so that the game won't be slowed down by searching for
        // asset Fcs as soon as the game loads.
        private static readonly bool _readAssetFlowchartsInEditor = true;

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
                FlowchartSignals.FlowchartEnabled += OnFlowchartEnabled;
                FlowchartSignals.FlowchartDestroyed += OnFlowchartDestroyed;
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                FlowchartSignals.FlowchartEnabled -= OnFlowchartEnabled;
                FlowchartSignals.FlowchartDestroyed -= OnFlowchartDestroyed;
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            ToggleEditorSubs(on);
        }

        private static void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            CaptureExistingFlowcharts();
        }

        private static void OnFlowchartEnabled(Flowchart flowchart)
        {
            if (IsPrefabModeFlowchart(flowchart))
            {
                RegisterFlowchart(flowchart, _prefabModeFlowcharts);
                return;
            }

            if (IsSceneFlowchart(flowchart))
            {
                RegisterFlowchart(flowchart, _sceneFlowcharts);
                return;
            }

            if (IsAssetFlowchart(flowchart))
            {
                RegisterFlowchart(flowchart, _assetFlowcharts);
            }
        }

        private static void OnFlowchartDestroyed(Flowchart flowchart)
        {
            UnregisterFlowchart(flowchart);
        }

        private static void CaptureExistingFlowcharts()
        {
            _flowchartLookup.Clear();
            _sceneFlowcharts.Clear();
            _assetFlowcharts.Clear();
            _prefabModeFlowcharts.Clear();

            CaptureLoadedFlowcharts();
            CaptureResourceFlowcharts();
            FullRefreshed();
        }

        public static event Action FullRefreshed = delegate { };

        private static void CaptureLoadedFlowcharts()
        {
            CaptureEditorFlowcharts();
            if (!Application.isPlaying)
            {
                return;
            }

            CaptureSceneFlowcharts();
        }

        private static void CaptureEditorFlowcharts()
        {
#if UNITY_EDITOR
            Flowchart[] found = Resources.FindObjectsOfTypeAll<Flowchart>();
            for (int i = 0; i < found.Length; i++)
            {
                Flowchart flowchart = found[i];
                if (IsPrefabModeFlowchart(flowchart))
                {
                    RegisterFlowchart(flowchart, _prefabModeFlowcharts);
                    continue;
                }

                if (IsSceneFlowchart(flowchart))
                {
                    RegisterFlowchart(flowchart, _sceneFlowcharts);
                }
            }
#endif
        }

        private static void CaptureSceneFlowcharts()
        {
            Flowchart[] flowcharts = UnityObj.FindObjectsByType<Flowchart>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            for (int i = 0; i < flowcharts.Length; i++)
            {
                RegisterFlowchart(flowcharts[i], _sceneFlowcharts);
            }
        }

        private static void CaptureResourceFlowcharts()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && _readAssetFlowchartsInEditor)
            {
                CaptureResourceFlowchartsInEditor();
                return;
            }
#endif
            if (Application.isPlaying && _readAssetFlowchartsInRuntime)
            {
                CaptureResourceFlowchartsAtRuntime();
            }
        }

#if UNITY_EDITOR
        private static void CaptureResourceFlowchartsInEditor()
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (!IsResourcePath(path))
                {
                    continue;
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    continue;
                }

                RegisterFlowchartsInPrefab(prefab);
            }
        }
#endif

        private static void CaptureResourceFlowchartsAtRuntime()
        {
            GameObject[] prefabs = Resources.LoadAll<GameObject>(string.Empty);
            // ^Note that Resources.LoadAll returns only the top-level assets,
            // so we have to iterate through them and find any nested Flowcharts.
            for (int i = 0; i < prefabs.Length; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null)
                {
                    continue;
                }

                RegisterFlowchartsInPrefab(prefab);
            }
        }

        private static void RegisterFlowchartsInPrefab(GameObject prefab)
        {
            Flowchart[] flowcharts = prefab.GetComponentsInChildren<Flowchart>(true);
            for (int i = 0; i < flowcharts.Length; i++)
            {
                RegisterFlowchart(flowcharts[i], _assetFlowcharts);
            }
        }

        private static void RegisterFlowchart(Flowchart flowchart, IDictionary<string, Flowchart> lookup)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
                lookup[flowchart.UniqueId] = flowchart;
                _flowchartLookup[flowchart.UniqueId] = flowchart;
            }
        }

        private static void UnregisterFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || string.IsNullOrEmpty(flowchart.UniqueId))
            {
                return;
            }

            lock (syncLock)
            {
                //Debug.Log($"Unregistering Flowchart {flowchart.name} from registry");
                _sceneFlowcharts.Remove(flowchart.UniqueId);
                _assetFlowcharts.Remove(flowchart.UniqueId);
                _prefabModeFlowcharts.Remove(flowchart.UniqueId);
                _flowchartLookup.Remove(flowchart.UniqueId);
            }
        }

        private static bool IsSceneFlowchart(Flowchart flowchart)
        {
            if (flowchart == null || flowchart.gameObject == null)
            {
                return false;
            }

            var scene = flowchart.gameObject.scene;
            return scene.IsValid() && scene.isLoaded && !IsPrefabModeFlowchart(flowchart);
        }

        private static bool IsAssetFlowchart(Flowchart flowchart)
        {
            // Don't worry about what this will do in runtime. On init, this class
            // gathers up all the FCs under Resources folders and registers them
            // as asset flowcharts. During gameplay, we won't be loading any new ones,
            // so we won't be missing any asset flowcharts by relying on this method 
            // in response to OnEnable.
#if UNITY_EDITOR
            return flowchart != null && EditorUtility.IsPersistent(flowchart);
#else
            return false;
#endif
        }

        private static bool IsPrefabModeFlowchart(Flowchart flowchart)
        {
#if UNITY_EDITOR
            if (flowchart == null || flowchart.gameObject == null)
            {
                return false;
            }

            return PrefabStageUtility.GetPrefabStage(flowchart.gameObject) != null;
#else
            return false;
#endif
        }

        private static bool IsResourcePath(string assetPath)
        {
            return assetPath.IndexOf(ResourcesFolderToken, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static readonly string ResourcesFolderToken = "/Resources/";
        private static readonly object syncLock = new object();

        // Keyed by unique id, which is generated on the Flowchart component and remains
        // consistent across scene loads and prefab instances.
        private static readonly Dictionary<string, Flowchart> _flowchartLookup =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Flowchart> _sceneFlowcharts =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Flowchart> _assetFlowcharts =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Flowchart> _prefabModeFlowcharts =
            new Dictionary<string, Flowchart>(StringComparer.Ordinal);

        public static IReadOnlyList<Flowchart> GetFlowcharts()
        {
            lock (syncLock)
            {
                return _flowchartLookup.Values.ToList();
            }
        }

        public static IReadOnlyList<Flowchart> GetSceneFlowcharts()
        {
            lock (syncLock)
            {
                return _sceneFlowcharts.Values.ToList();
            }
        }

        public static IReadOnlyList<Flowchart> GetAssetFlowcharts()
        {
            lock (syncLock)
            {
                return _assetFlowcharts.Values.ToList();
            }
        }

        public static IReadOnlyList<Flowchart> GetPrefabModeFlowcharts()
        {
            lock (syncLock)
            {
                return _prefabModeFlowcharts.Values.ToList();
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
                _flowchartLookup.TryGetValue(guid, out Flowchart flowchart);
                return flowchart;
            }
        }

        public static void Clear()
        {
            lock (syncLock)
            {
                _flowchartLookup.Clear();
                _sceneFlowcharts.Clear();
                _assetFlowcharts.Clear();
                _prefabModeFlowcharts.Clear();
            }
        }

        private static void ToggleEditorSubs(bool on)
        {
#if UNITY_EDITOR

            if (on)
            {
                EditorSceneManager.sceneOpened += OnEditorSceneOpened;
                EditorSceneManager.sceneClosed += OnEditorSceneClosed;
            }
            else
            {
                EditorSceneManager.sceneOpened -= OnEditorSceneOpened;
                EditorSceneManager.sceneClosed -= OnEditorSceneClosed;
            }
#endif
        }

        private static void OnEditorSceneClosed(Scene scene)
        {
            CaptureExistingFlowcharts();
        }

        private static void OnEditorSceneOpened(Scene scene, OpenSceneMode mode)
        {
            CaptureExistingFlowcharts();
        }
    }
}