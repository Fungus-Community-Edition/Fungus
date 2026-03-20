#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AtMycelia.Amanita.VScripting;

namespace AtMycelia.Amanita.EditorUtils
{
    [InitializeOnLoad]
    public static class FlowchartVariableManagerInitializer
    {
        static FlowchartVariableManagerInitializer()
        {
            EditorApplication.delayCall += InitializeFlowcharts;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosed += OnSceneClosed;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            InitializeFlowcharts();
        }

        private static void OnSceneClosed(Scene scene)
        {
            InitializeFlowcharts();
        }

        private static void InitializeFlowcharts()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var flowcharts = Resources.FindObjectsOfTypeAll<Flowchart>();
            if (flowcharts == null || flowcharts.Length == 0)
            {
                return;
            }

            foreach (var flowchart in flowcharts)
            {
                if (flowchart == null || flowchart.gameObject == null)
                {
                    continue;
                }

                var scene = flowchart.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                flowchart.EnsureVariableManagerMigrationForEditor(out bool migrated);
                if (migrated)
                {
                    EditorUtility.SetDirty(flowchart);
                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }
    }
}
#endif
