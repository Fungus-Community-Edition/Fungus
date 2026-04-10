using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [InitializeOnLoad]
    internal static class FlowchartDomainReloadHandler
    {
        static FlowchartDomainReloadHandler()
        {
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            DelayedRefresh();
        }

        private static void DelayedRefresh()
        {
            EditorApplication.delayCall += RefreshAllFlowcharts;
        }

        private static void OnAfterAssemblyReload()
        {
            DelayedRefresh();
        }

        private static void RefreshAllFlowcharts()
        {
            var flowcharts = Resources.FindObjectsOfTypeAll<Flowchart>()
                .Where(fc => fc != null && !EditorUtility.IsPersistent(fc.gameObject));
            
            foreach (var flowchart in flowcharts)
            {
                flowchart.RefreshVariableManagerForEditorReload();
            }

            GameObject selected = Selection.activeGameObject;
            if (selected != null && selected.TryGetComponent<Flowchart>(out _))
            {
                // Deselect the GameObject to force the Inspector to refresh, then reselect it.
                // Otherwise, the Inspector may make it look like a Flowchart lost its
                // variables when in reality, it just needed a refresh.
                Selection.activeGameObject = null;
                EditorApplication.delayCall += () => Selection.activeGameObject = selected;
            }
        }
    }
}