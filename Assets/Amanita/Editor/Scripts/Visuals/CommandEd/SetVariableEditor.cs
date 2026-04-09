using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Hyphlow;
using Type = System.Type;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor(typeof(SetVariable))]
    public class SetVariableEditor : CommandEditor
    {
        protected SerializedProperty anyVarDataPairProp;
        protected SerializedProperty anyVarDataProp;
        protected SerializedProperty setOperatorProp;
        protected SerializedProperty lhsVarProp;

        public override void OnEnable()
        {
            base.OnEnable();

            anyVarDataPairProp = serializedObject.FindProperty("_anyVar");
            lhsVarProp = anyVarDataPairProp.FindPropertyRelative("varRef"); // VariableReference
            anyVarDataProp = anyVarDataPairProp.FindPropertyRelative("data"); // AnyVariableData
            setOperatorProp = serializedObject.FindProperty("_setOperator");
        }

        public override void DrawCommandGUI()
        {
            setVarCommand = (SetVariable)target;
            flowchart = setVarCommand.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }

            // Draw and ensure LHS VariableReference has an owner
            HandleLhsVarField();

            // Build and draw operator selector
            DrawSetOperatorField();

            // Apply chosen operator
            ApplySetOperatorChoice();

            // Draw RHS value field (and ensure correct IVariableData type without boxedValue)
            HandleRhsValueField();

            // Commit changes
            serializedObject.Update();
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        protected Flowchart flowchart;
        protected SetVariable setVarCommand;

        protected IVariable selectedVariable;
        protected int selectedOpIndex;
        protected readonly List<GUIContent> operatorsList = new List<GUIContent>();
        protected readonly List<SetOperator> operatorValues = new List<SetOperator>();

        protected virtual void HandleLhsVarField()
        {
            // Draw the VariableReference field via its drawer (lets user pick the variable)
            EditorGUILayout.PropertyField(lhsVarProp, new GUIContent("Var to Set"));

            // Ensure owner is set in the serialized fields (avoid touching boxedValue)
            var owningSourceProp = lhsVarProp.FindPropertyRelative("owningSource");
            if (owningSourceProp != null && owningSourceProp.objectReferenceValue == null && flowchart != null)
            {
                owningSourceProp.objectReferenceValue = flowchart;
            }
            IVariableSource owner = owningSourceProp != null ? 
                owningSourceProp.objectReferenceValue as IVariableSource : 
                null;
            // Resolve selected variable purely from serialized fields (no boxedValue)
            var itemIdProp = lhsVarProp.FindPropertyRelative("itemId");
            selectedVariable = null;
            if (owner != null && itemIdProp != null)
            {
                byte itemId = (byte)itemIdProp.intValue; // Unity stores byte as int internally
                selectedVariable = owner.GetVariable(itemId);
            }
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
            if (operatorValues.Count > 0)
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

        protected void TryAdd(SetOperator op)
        {
            if (selectedVariable != null && selectedVariable.IsArithmeticSupported(op))
            {
                operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(op)));
                operatorValues.Add(op);
            }
        }

        protected virtual void ApplySetOperatorChoice()
        {
            bool weHaveValidSetOp = selectedVariable != null &&
                                    operatorValues.Count > 0 &&
                                    selectedOpIndex >= 0 &&
                                    selectedOpIndex < operatorValues.Count;
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

            // Ensure AnyVariableData.data (SerializeReference) is of the correct IVariableData type
            // without touching boxedValue. We replace the managed reference when needed.
            var innerDataRefProp = anyVarDataProp != null
                ? anyVarDataProp.FindPropertyRelative("data") // SerializeReference IVariableData
                : null;

            if (innerDataRefProp != null)
            {
                object current = innerDataRefProp.managedReferenceValue;
                var varType = selectedVariable.GetType();
                Type desiredDataType = VariableDataTypeRegistry.CreateForVar(varType)?.GetType();

                if (desiredDataType != null)
                {
                    bool needsReplace = current == null || current.GetType() != desiredDataType;
                    if (needsReplace)
                    {
                        // Create a fresh IVariableData instance of the right type
                        object replacement = System.Activator.CreateInstance(desiredDataType);
                        innerDataRefProp.managedReferenceValue = replacement;
                        // After replacing the managed reference, we must re-fetch nested properties.
                        serializedObject.ApplyModifiedProperties();
                        serializedObject.Update();
                    }
                }
            }

            // Now draw the concrete inner data: anyVar.data.data
            // Re-fetch in case we just replaced the managed reference
            var rhsVarDataPropLocal = anyVarDataPairProp.FindPropertyRelative("data.data");
            if (rhsVarDataPropLocal != null)
            {
                EditorGUILayout.PropertyField(rhsVarDataPropLocal, _valueToApplyLabel, true);
            }
            else
            {
                EditorGUILayout.HelpBox("Unable to locate RHS data. Select a variable first.", 
                    MessageType.Warning);
            }
        }

        protected static GUIContent _valueToApplyLabel = new GUIContent("Value to Apply");
    }
}
