using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class PrefabVariableOverrideUtility
    {
        private const string FlowchartMenuPath =
            "CONTEXT/Flowchart/Apply Prefab Overrides (Keep Scene Variable References)";
        private const string VarManagerMenuPath =
            "CONTEXT/VariableManagerComponent/Apply Prefab Overrides (Keep Scene Variable References)";

        [MenuItem(FlowchartMenuPath)]
        private static void ApplyOverridesFromFlowchart(MenuCommand command)
        {
            Flowchart flowchart = command.context as Flowchart;
            ApplyOverrides(flowchart);
        }

        [MenuItem(FlowchartMenuPath, true)]
        private static bool ValidateOverridesFromFlowchart(MenuCommand command)
        {
            Flowchart flowchart = command.context as Flowchart;
            return IsPrefabInstance(flowchart != null ? flowchart.gameObject : null);
        }

        [MenuItem(VarManagerMenuPath)]
        private static void ApplyOverridesFromVarManager(MenuCommand command)
        {
            VariableManagerComponent component = command.context as VariableManagerComponent;
            Flowchart flowchart = component != null ? component.GetComponent<Flowchart>() : null;
            ApplyOverrides(flowchart);
        }

        [MenuItem(VarManagerMenuPath, true)]
        private static bool ValidateOverridesFromVarManager(MenuCommand command)
        {
            VariableManagerComponent component = command.context as VariableManagerComponent;
            return IsPrefabInstance(component != null ? component.gameObject : null);
        }

        private static bool IsPrefabInstance(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            return PrefabUtility.IsPartOfPrefabInstance(instance) &&
                   !PrefabUtility.IsPartOfPrefabAsset(instance);
        }

        private static void ApplyOverrides(Flowchart flowchart)
        {
            if (flowchart == null)
            {
                Debug.LogWarning("Apply Overrides failed: Flowchart was null.");
                return;
            }

            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(flowchart.gameObject);
            VariableManagerComponent varManager = flowchart.GetComponent<VariableManagerComponent>();
            if (varManager == null)
            {
                Debug.LogWarning("Apply Overrides failed: VariableManagerComponent not found.");
                return;
            }

            List<SceneVarReference> sceneRefs = CaptureSceneVariableReferences(varManager);

            PrefabUtility.ApplyPrefabInstance(root, InteractionMode.UserAction);

            RestoreSceneVariableReferences(varManager, sceneRefs);
        }

        private static List<SceneVarReference> CaptureSceneVariableReferences(VariableManagerComponent varManager)
        {
            List<SceneVarReference> result = new List<SceneVarReference>();
            IReadOnlyList<IVariable> variables = varManager.Variables;

            for (int i = 0; i < variables.Count; i++)
            {
                IVariable variable = variables[i];
                if (variable == null)
                {
                    continue;
                }

                UnityObj obj = variable.BoxedValue as UnityObj;
                if (obj == null)
                {
                    continue;
                }

                if (EditorUtility.IsPersistent(obj))
                {
                    continue;
                }

                result.Add(new SceneVarReference(variable.ItemId, variable.Key, variable.ContentType, obj));
            }

            return result;
        }

        private static void RestoreSceneVariableReferences(
            VariableManagerComponent varManager,
            List<SceneVarReference> sceneRefs)
        {
            if (sceneRefs == null || sceneRefs.Count == 0)
            {
                return;
            }

            bool changedAny = false;

            for (int i = 0; i < sceneRefs.Count; i++)
            {
                SceneVarReference sceneRef = sceneRefs[i];
                IVariable variable = varManager.GetVariable(sceneRef.ItemId);

                if (variable == null && !string.IsNullOrEmpty(sceneRef.Key))
                {
                    variable = varManager.GetVariable(sceneRef.Key, StringComparison.Ordinal);
                }

                if (variable == null || sceneRef.Value == null)
                {
                    continue;
                }

                if (!sceneRef.ContentType.IsAssignableFrom(sceneRef.Value.GetType()))
                {
                    continue;
                }

                if (!Equals(variable.BoxedValue, sceneRef.Value))
                {
                    Undo.RecordObject(ResolveRecordTarget(varManager, variable),
                        $"Restore {variable.ContentType.Name} Variable");

                    variable.BoxedValue = sceneRef.Value;
                    changedAny = true;
                }
            }

            if (changedAny)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(varManager);
                EditorUtility.SetDirty(varManager);

                if (varManager.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(varManager.gameObject.scene);
                }
            }
        }

        private static UnityObj ResolveRecordTarget(VariableManagerComponent manager, IVariable variable)
        {
            UnityObj direct = variable as UnityObj;
            if (direct != null)
            {
                return direct;
            }

            return manager;
        }

        private readonly struct SceneVarReference
        {
            public SceneVarReference(byte itemId, string key, Type contentType, UnityObj value)
            {
                ItemId = itemId;
                Key = key;
                ContentType = contentType;
                Value = value;
            }

            public byte ItemId { get; }
            public string Key { get; }
            public Type ContentType { get; }
            public UnityObj Value { get; }
        }
    }
}