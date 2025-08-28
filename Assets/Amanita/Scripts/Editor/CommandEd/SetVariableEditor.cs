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

            //fetching every draw to ensure we don't have stale data based on types that have changed by user selection,
            //  without us noticing.

            Variable selectedVariable = anyVarProp.FindPropertyRelative("variable").objectReferenceValue as Variable;
            IList<GUIContent> operatorsList = new List<GUIContent>();
            PopulateOperatorsList();
            void PopulateOperatorsList()
            {
                if (selectedVariable != null)
                {
                    if (selectedVariable.IsArithmeticSupported(SetOperator.Assign))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Assign)));

                    if (selectedVariable.IsArithmeticSupported(SetOperator.Negate))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Negate)));

                    if (selectedVariable.IsArithmeticSupported(SetOperator.Add))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Add)));

                    if (selectedVariable.IsArithmeticSupported(SetOperator.Subtract))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Subtract)));

                    if (selectedVariable.IsArithmeticSupported(SetOperator.Multiply))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Multiply)));

                    if (selectedVariable.IsArithmeticSupported(SetOperator.Divide))
                        operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(SetOperator.Divide)));
                }
                else
                {
                    operatorsList.Add(VariableConditionEditor.None);
                }
            }

            SetOperator prevOperator = setVarCommand.SetOperator;
            int selectedIndex = (int)setVarCommand.SetOperator;
            bool varSupportsOperator = selectedVariable != null && selectedVariable.IsArithmeticSupported(prevOperator);
            if (!varSupportsOperator) // <- This can occur when changing between variable types
            {
                selectedIndex = 0;
            }

            GetAndShowCurrentOperator();
            void GetAndShowCurrentOperator()
            {
                GUIContent operatorContent = new GUIContent("Operation", "Arithmetic operator to use");
                selectedIndex = EditorGUILayout.Popup(operatorContent, selectedIndex, operatorsList.ToArray());
            }

            if (selectedVariable != null)
            {
                setOperatorProp.enumValueIndex = selectedIndex;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
