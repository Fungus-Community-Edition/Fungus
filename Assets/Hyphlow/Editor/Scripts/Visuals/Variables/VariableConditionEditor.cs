using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Handles custom drawing for ConditionExperssions within the VariableCondition and inherited commands.
    /// 
    /// TODO; refactor to allow a propertydrawer on ConditionExperssion and potentially list as reorderable
    /// </summary>
    [CustomEditor(typeof(VariableCondition), true)]
    public class VariableConditionEditor : CommandEditor
    {
        public static readonly GUIContent None = new GUIContent("<None>");

        public static readonly GUIContent[] emptyList = new GUIContent[]
        {
            None,
        };

        private static readonly GUIContent[] compareListAll = new GUIContent[]
        {
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.Equals)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.NotEquals)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.LessThan)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.GreaterThan)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.LessThanOrEquals)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.GreaterThanOrEquals)),
        };

        private static readonly GUIContent[] compareListEqualOnly = new GUIContent[]
        {
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.Equals)),
            new GUIContent(VariableUtil.GetCompareOperatorDescription(CompareOperator.NotEquals)),
        };

        public override void OnEnable()
        {
            base.OnEnable();

            conditions = serializedObject.FindProperty("_conditions");
            anyOrAllConditions = serializedObject.FindProperty("_anyOrAllConditions");
        }

        protected SerializedProperty conditions;
        protected SerializedProperty anyOrAllConditions;

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(anyOrAllConditions);

            int newSize = EditorGUILayout.IntField("Size", conditions.arraySize);
            bool sizeChanged = newSize != conditions.arraySize;
            if (sizeChanged)
            {
                conditions.arraySize = Mathf.Max(0, newSize);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            GUILayout.Label("Conditions", EditorStyles.boldLabel);

            VariableCondition condTarget = target as VariableCondition;

            var flowchart = condTarget.GetFlowchart();
            if (flowchart == null)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < conditions.arraySize; i++)
            {
                var conditionAnyVar = conditions.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("_anyVar");
                var varRefProp = conditionAnyVar.FindPropertyRelative("_varRef");
                var varDataProp = conditionAnyVar.FindPropertyRelative("_data._data");
                var conditionCompare = conditions.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("_compareOperator");

                EditorGUILayout.PropertyField(varRefProp, new GUIContent("Lhs"), true);

                var varRef = varRefProp.boxedValue as VariableReference;
                IVariable selectedVariable = varRef?.Variable;

                if (selectedVariable == null)
                {
                    EditorGUILayout.Separator();
                    continue;
                }

                EnsureRhsDataInitialized(conditionAnyVar, selectedVariable);

                GUIContent[] operatorsList;
                if (selectedVariable.IsComparisonSupported())
                {
                    operatorsList = compareListAll;
                }
                else
                {
                    operatorsList = compareListEqualOnly;
                }

                // Get previously selected operator
                int selectedIndex = conditionCompare.enumValueIndex;
                if (selectedIndex < 0 || selectedIndex >= operatorsList.Length)
                {
                    // Default to first index if the operator is not found in the available operators list
                    // This can occur when changing between variable types
                    selectedIndex = 0;
                }

                selectedIndex = EditorGUILayout.Popup(
                    new GUIContent("Operator", "The comparison operator to use when comparing values"),
                    selectedIndex,
                    operatorsList);

                conditionCompare.enumValueIndex = selectedIndex;

                EditorGUILayout.PropertyField(varDataProp, new GUIContent("Rhs"), true);
                EditorGUILayout.Separator();
                
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        private static void EnsureRhsDataInitialized(SerializedProperty conditionAnyVar, 
            IVariable selectedVariable)
        {
            if (selectedVariable == null || conditionAnyVar == null)
            {
                return;
            }

            AnyVariableAndDataPair pairInstance = conditionAnyVar.boxedValue as AnyVariableAndDataPair;
            if (pairInstance == null)
            {
                return;
            }

            AnyVariableData anyVarData = pairInstance.Data;
            if (anyVarData == null)
            {
                return;
            }

            Type effectiveVarType = GetEffectiveVarType(selectedVariable);
            if (effectiveVarType == null)
            {
                return;
            }

            anyVarData.SetFor(effectiveVarType, selectedVariable.ContentType);
            pairInstance.Data = anyVarData;
            conditionAnyVar.boxedValue = pairInstance;
            conditionAnyVar.serializedObject.ApplyModifiedProperties();
        }

        private static Type GetEffectiveVarType(IVariable variable)
        {
            if (variable is IVariablePointer ptr && ptr.Component is IVariable inner)
            {
                return inner.GetType();
            }

            return variable?.GetType();
        }
    }
}
