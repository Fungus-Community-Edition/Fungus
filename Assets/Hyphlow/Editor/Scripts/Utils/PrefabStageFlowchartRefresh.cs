using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// This is to fix the variable-disappearance issue that can happen when displaying
    /// a Flowchart in a prefab stage. When a prefab stage is opened, this will refresh all
    /// Flowcharts within the prefab stage to ensure their variables are correctly displayed.
    /// </summary>
    [InitializeOnLoad]
    internal static class PrefabStageFlowchartRefresh
    {
        static PrefabStageFlowchartRefresh()
        {
            PrefabStage.prefabStageOpened -= OnPrefabStageOpened;
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        }

        private static void OnPrefabStageOpened(PrefabStage stage)
        {
            if (stage == null || stage.prefabContentsRoot == null)
            {
                return;
            }

            var flowcharts = stage.prefabContentsRoot.GetComponentsInChildren<Flowchart>(true);
            for (int i = 0; i < flowcharts.Length; i++)
            {
                var flowchart = flowcharts[i];
                if (flowchart == null || flowchart.VariableCount > 0)
                {
                    continue;
                }

                flowchart.RefreshVariableManagerForEditorReload();
                EditorUtility.SetDirty(flowchart);
            }

            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                Selection.activeGameObject = null;
                EditorApplication.delayCall += () => Selection.activeGameObject = selected;
            }
        }
    }
}