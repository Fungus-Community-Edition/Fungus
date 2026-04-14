#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [InitializeOnLoad]
    public static class FlowchartVariableManagerInitializer
    {
        static FlowchartVariableManagerInitializer()
        {
            DelayedInitializeFlowcharts();
            ToggleSubs(false);
            ToggleSubs(true);
        }

        private static void DelayedInitializeFlowcharts()
        {
            EditorApplication.delayCall += InitializeFlowcharts;
        }

        private static void OnAfterAssemblyReload()
        {
            DelayedInitializeFlowcharts();//
        }

        private static void ToggleSubs(bool on)
        {
            if (on)
            {
                AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
                EditorSceneManager.sceneOpened += OnSceneOpened;
                EditorSceneManager.sceneClosed += OnSceneClosed;
            }
            else
            {
                AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
                EditorSceneManager.sceneOpened -= OnSceneOpened;
                EditorSceneManager.sceneClosed -= OnSceneClosed;
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            DelayedInitializeFlowcharts();
        }

        private static void OnSceneClosed(Scene scene)
        {
            DelayedInitializeFlowcharts();
        }

        private static void InitializeFlowcharts()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var flowcharts = UnityObj.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
            if (flowcharts == null || flowcharts.Length == 0)
            {
                return;
            }

            foreach (var elem in flowcharts)
            {
                if (elem == null || elem.gameObject == null)
                {
                    continue;
                }

                var scene = elem.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

            }
        }
    }
}
#endif
