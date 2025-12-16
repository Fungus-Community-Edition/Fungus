using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Amanita.VScripting.Commands;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor(typeof(SetVariable))]
    public class SetVariableEditor : CommandEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();

            anyVarDataPairProp = serializedObject.FindProperty("anyVar");
            anyVarDataProp = serializedObject.FindProperty("anyVar.data");
            rhsVarDataProp = serializedObject.FindProperty("anyVar.data.data");

            setOperatorProp = serializedObject.FindProperty("setOperator");
            lhsVarProp = serializedObject.FindProperty("varToSet");
        }

        protected SerializedProperty anyVarDataPairProp;
        protected SerializedProperty anyVarDataProp;
        protected SerializedProperty rhsVarDataProp;
        protected SerializedProperty setOperatorProp;
        protected SerializedProperty lhsVarProp;

        public override void DrawCommandGUI()
        {
            setVarCommand = target as SetVariable;
            flowchart = setVarCommand.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            HandleLhsVarField();
            DrawSetOperatorField();
            ApplySetOperatorChoice();
            HandleRhsValueField();

            serializedObject.Update();
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
                
        }

        protected Flowchart flowchart;
        SetVariable setVarCommand;

        protected virtual void HandleLhsVarField()
        {
            EditorGUILayout.PropertyField(lhsVarProp, new GUIContent("Var to Set"));
            lhsVarProp.serializedObject.Update();
            var varRefForSet = lhsVarProp.boxedValue as VariableReference;

            EnsureRefHasOwner();
            void EnsureRefHasOwner()
            {
                if (varRefForSet.VarOwner == null)
                {
                    varRefForSet.VarOwner = flowchart;
                    lhsVarProp.boxedValue = varRefForSet; // To make sure it sticks
                    lhsVarProp.serializedObject.ApplyModifiedProperties();
                }
            }

            selectedVariable = varRefForSet?.Variable;
        }

        protected virtual void DrawSetOperatorField()
        {
            operatorsList.Clear();
            operatorValues.Clear();

            if (selectedVariable != null)
            {
                TryAdd(SetOperator.Assign);
                TryAdd(SetOperator.Negate);
                TryAdd(SetOperator.Add);
                TryAdd(SetOperator.Subtract);
                TryAdd(SetOperator.Multiply);
                TryAdd(SetOperator.Divide);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a variable to see available operations.", MessageType.Info);
                return;
            }

            // Determine current selection index
            if (selectedVariable != null && operatorValues.Count > 0)
            {
                var currentOp = setVarCommand.SetOperator;
                int idx = operatorValues.IndexOf(currentOp);
                selectedOpIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                selectedOpIndex = 0;
            }

            // Show popup
            GUIContent operatorContent = new GUIContent("Operation", "Arithmetic operator to use");
            selectedOpIndex = EditorGUILayout.Popup(operatorContent, selectedOpIndex, operatorsList.ToArray());
        }

        IVariable selectedVariable;
        int selectedOpIndex;
        readonly List<GUIContent> operatorsList = new List<GUIContent>();
        readonly List<SetOperator> operatorValues = new List<SetOperator>();

        void TryAdd(SetOperator op)
        {
            if (selectedVariable.IsArithmeticSupported(op))
            {
                operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(op)));
                operatorValues.Add(op);
            }
        }

        protected virtual void ApplySetOperatorChoice()
        {
            bool weHaveValidSetOp = selectedVariable != null && operatorValues.Count > 0
                    && selectedOpIndex >= 0 && selectedOpIndex < operatorValues.Count;
            if (weHaveValidSetOp)
            {
                SetOperator chosenOp = operatorValues[selectedOpIndex];
                setOperatorProp.enumValueIndex = (int)chosenOp;
            }
        }

        protected virtual void HandleRhsValueField()
        {
            if (selectedVariable == null)
            {
                return;
            }

            var anyVarData = anyVarDataProp.boxedValue as AnyVariableData;
            bool needUpdateVarDataType = !anyVarData.ContentType.Equals(selectedVariable.ContentType);
            if (needUpdateVarDataType)
            {
                anyVarData.SetFor(selectedVariable.GetType(), selectedVariable.ContentType);
                anyVarDataProp.boxedValue = anyVarData;
                anyVarDataProp.serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.PropertyField(rhsVarDataProp, new GUIContent("Value to Apply"), true);
        }
    }
}
