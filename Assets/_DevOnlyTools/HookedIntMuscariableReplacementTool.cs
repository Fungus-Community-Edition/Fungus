using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class HookedIntMuscariableReplacementTool
    {
        [MenuItem("Tools/AtMycelia/Debug/Replace HookedInt Muscariables", false, 2000)]
        private static void ReplaceHookedIntMuscariables()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("HookedIntMuscariable replacement cannot run in Play Mode.");
                return;
            }

            Type hookedType = FindHookedIntType();
            if (hookedType == null)
            {
                Debug.LogWarning("HookedIntMuscariable type not found. No replacements were made.");
                return;
            }

            int replacedCount = 0;
            int managerCount = 0;

            ReplaceInLoadedScenes(hookedType, ref replacedCount, ref managerCount);
            ReplaceInPrefabs(hookedType, ref replacedCount, ref managerCount);

            Debug.Log($"Replaced {replacedCount} HookedIntMuscariable(s) across " +
                $"{managerCount} VariableManagerComponent(s).");
        }

        private static void ReplaceInLoadedScenes(Type hookedType, ref int replacedCount, ref int managerCount)
        {
            VariableManagerComponent[] managers = UnityObj.FindObjectsByType<VariableManagerComponent>(FindObjectsSortMode.None);

            foreach (VariableManagerComponent manager in managers)
            {
                if (manager == null)
                {
                    continue;
                }

                if (!manager.gameObject.scene.IsValid() || !manager.gameObject.scene.isLoaded)
                {
                    continue;
                }

                int replacedInManager = ReplaceInManager(manager, hookedType);
                if (replacedInManager > 0)
                {
                    replacedCount += replacedInManager;
                    managerCount++;
                    EditorUtility.SetDirty(manager);
                }
            }
        }

        private static void ReplaceInPrefabs(Type hookedType, ref int replacedCount, ref int managerCount)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);

                try
                {
                    VariableManagerComponent[] managers = prefabRoot.GetComponentsInChildren<VariableManagerComponent>(true);
                    bool anyChanges = false;

                    foreach (VariableManagerComponent manager in managers)
                    {
                        int replacedInManager = ReplaceInManager(manager, hookedType);
                        if (replacedInManager > 0)
                        {
                            replacedCount += replacedInManager;
                            managerCount++;
                            anyChanges = true;
                        }
                    }

                    if (anyChanges)
                    {
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                    }
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(prefabRoot);
                }
            }
        }

        private static int ReplaceInManager(VariableManagerComponent manager, Type hookedType)
        {
            List<IVariable> variables = manager.Variables.ToList();
            int replaced = 0;

            for (int i = 0; i < variables.Count; i++)
            {
                IVariable toReplace = variables[i];
                if (toReplace == null || !hookedType.IsInstanceOfType(toReplace))
                {
                    continue;
                }

                Muscariable replacement = VariableFactory.CreateByContentType(typeof(int), toReplace);
                if (replacement == null)
                {
                    Debug.LogWarning($"Failed to replace HookedIntMuscariable in {manager.name}.");
                    continue;
                }

                variables[i] = replacement;
                replaced++;
            }

            if (replaced > 0)
            {
                manager.ReorderVariables(variables);
                manager.Refresh();

                if (manager.Owner is UnityObj ownerObj)
                {
                    EditorUtility.SetDirty(ownerObj);
                }
            }

            return replaced;
        }

        private static Type FindHookedIntType()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("VScriptingTests.VariableOperations.HookedIntMuscariable"))
                .FirstOrDefault(type => type != null);
        }
    }
}