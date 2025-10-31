using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Amanita.VScripting.Commands;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{

    [CustomEditor (typeof(SetVariable))]
    public class SetVariableEditor : CommandEditor
    {
        protected SerializedProperty anyVarProp;
        protected SerializedProperty setOperatorProp;
        
        public override void OnEnable()
        {
            base.OnEnable();

            anyVarProp = serializedObject.FindProperty("anyVar");
            setOperatorProp = serializedObject.FindProperty("setOperator");
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            SetVariable setVarCommand = target as SetVariable;

            var flowchart = setVarCommand.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            // Select Variable
            EditorGUILayout.PropertyField(anyVarProp, true);

            // Read selected variable safely (ManagedReference or ObjectReference)
            var variableProp = anyVarProp.FindPropertyRelative("variable");
            IVariable selectedVariable = ReadIVariable(variableProp);

            // Build operators list + parallel enum list for correct mapping
            var operatorsList = new List<GUIContent>();
            var operatorValues = new List<SetOperator>();

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
                operatorsList.Add(VariableConditionEditor.None);
            }

            void TryAdd(SetOperator op)
            {
                if (selectedVariable.IsArithmeticSupported(op))
                {
                    operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(op)));
                    operatorValues.Add(op);
                }
            }

            // Determine current selection index
            int selectedIndex;
            if (selectedVariable != null && operatorValues.Count > 0)
            {
                var currentOp = setVarCommand.SetOperator;
                int idx = operatorValues.IndexOf(currentOp);
                selectedIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                selectedIndex = 0;
            }

            // Show popup
            GUIContent operatorContent = new GUIContent("Operation", "Arithmetic operator to use");
            selectedIndex = EditorGUILayout.Popup(operatorContent, selectedIndex, operatorsList.ToArray());

            // Apply selection back to enum
            if (selectedVariable != null && operatorValues.Count > 0 && selectedIndex >= 0 && selectedIndex < operatorValues.Count)
            {
                setOperatorProp.enumValueIndex = (int)operatorValues[selectedIndex];
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static IVariable ReadIVariable(SerializedProperty prop)
        {
            if (prop == null) return null;
            if (prop.propertyType == SerializedPropertyType.ManagedReference)
            {
                return prop.managedReferenceValue as IVariable;
            }
            return prop.objectReferenceValue as IVariable;
        }
    }
}
