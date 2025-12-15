using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
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

        protected SerializedProperty conditions;

        public override void OnEnable()
        {
            base.OnEnable();

            conditions = serializedObject.FindProperty("conditions");
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("anyOrAllConditions"));

            conditions.arraySize = EditorGUILayout.IntField("Size", conditions.arraySize);
            GUILayout.Label("Conditions", EditorStyles.boldLabel);

            VariableCondition condTarget = target as VariableCondition;

            var flowchart = condTarget.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            EditorGUI.indentLevel++;
            for (int i = 0; i < conditions.arraySize; i++)
            {
                var conditionAnyVar = conditions.GetArrayElementAtIndex(i).FindPropertyRelative("anyVar");
                var conditionCompare = conditions.GetArrayElementAtIndex(i).FindPropertyRelative("compareOperator");

                EditorGUILayout.PropertyField(conditionAnyVar, new GUIContent("Variable"), true);

                // Get selected variable - support both UnityEngine.Object and POCOs via [SerializeReference]
                var varProp = conditionAnyVar.FindPropertyRelative("variable");
                IVariable selectedVariable = null;

                // UnityEngine.Object path
                bool varIsUnityObj = varProp != null && varProp.propertyType == SerializedPropertyType.ObjectReference &&
                    varProp.objectReferenceValue is UnityObj;
                if (varIsUnityObj)
                {
                    selectedVariable = varProp.objectReferenceValue as IVariable;
                }

                // [SerializeReference] path for POCOs (e.g., Muscariable)
                var varIsPoco = varProp != null && varProp.propertyType == SerializedPropertyType.ManagedReference;
                if (varIsPoco)
                {
                    selectedVariable = varProp.managedReferenceValue as IVariable;
                }

                if (selectedVariable == null)
                {
                    EditorGUILayout.Separator();
                    continue;
                }

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
                    new GUIContent("Compare", "The comparison operator to use when comparing values"),
                    selectedIndex,
                    operatorsList);

                conditionCompare.enumValueIndex = selectedIndex;

                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
