using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Type = System.Type;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor(typeof(SetVariable))]
    public class SetVariableEditor : CommandEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();
            _anyVarDataPairProp = serializedObject.FindProperty("_anyVar");
            _lhsVarProp = _anyVarDataPairProp.FindPropertyRelative("_varRef"); // VariableReference
            _anyVarDataProp = _anyVarDataPairProp.FindPropertyRelative("_data"); // AnyVariableData
            _setOperatorProp = serializedObject.FindProperty("_setOperator");
            _owningSourceProp = _lhsVarProp.FindPropertyRelative("_owningSource");
            _itemIdProp = _lhsVarProp.FindPropertyRelative("_itemId");
        }

        protected SerializedProperty _anyVarDataPairProp;
        protected SerializedProperty _anyVarDataProp;
        protected SerializedProperty _setOperatorProp;
        protected SerializedProperty _lhsVarProp;
        protected SerializedProperty _owningSourceProp;
        protected SerializedProperty _itemIdProp;

        public override void DrawCommandGUI()
        {
            _setVarCommand = (SetVariable)target;
            _flowchart = _setVarCommand.GetFlowchart();
            if (_flowchart == null)
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

        protected Flowchart _flowchart;
        protected SetVariable _setVarCommand;

        protected IVariable _selectedVariable;
        protected int _selectedOpIndex;
        protected readonly List<GUIContent> _operatorsList = new List<GUIContent>();
        protected readonly List<SetOperator> _operatorValues = new List<SetOperator>();

        protected virtual void HandleLhsVarField()
        {
            // Draw the VariableReference field via its drawer (lets user pick the variable)
            EditorGUILayout.PropertyField(_lhsVarProp, new GUIContent("Var to Set"));

            // Ensure owner is set in the serialized fields (avoid touching boxedValue)
            bool shouldAssignFlowchartAsOwner = _owningSourceProp != null && 
                _owningSourceProp.objectReferenceValue == null &&
                _flowchart != null;
            if (shouldAssignFlowchartAsOwner)
            {
                _owningSourceProp.objectReferenceValue = _flowchart;
            }
            IVariableSource owner = _owningSourceProp != null ? 
                _owningSourceProp.objectReferenceValue as IVariableSource : 
                null;
            // Resolve selected variable purely from serialized fields (no boxedValue)
            _selectedVariable = null;
            if (owner != null && _itemIdProp != null)
            {
                byte itemId = (byte)_itemIdProp.intValue; // Unity stores byte as int internally
                _selectedVariable = owner.GetVariable(itemId);
            }
        }

        protected virtual void DrawSetOperatorField()
        {
            _operatorsList.Clear();
            _operatorValues.Clear();

            if (_selectedVariable != null)
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
            if (_operatorValues.Count > 0)
            {
                var currentOp = _setVarCommand.SetOperator;
                int idx = _operatorValues.IndexOf(currentOp);
                _selectedOpIndex = idx >= 0 ? idx : 0;
            }
            else
            {
                _selectedOpIndex = 0;
            }

            // Show popup
            GUIContent operatorContent = new GUIContent("Operation", "Arithmetic operator to use");
            _selectedOpIndex = EditorGUILayout.Popup(operatorContent, _selectedOpIndex, _operatorsList.ToArray());
        }

        protected void TryAdd(SetOperator op)
        {
            if (_selectedVariable != null && _selectedVariable.IsArithmeticSupported(op))
            {
                _operatorsList.Add(new GUIContent(VariableUtil.GetSetOperatorDescription(op)));
                _operatorValues.Add(op);
            }
        }

        protected virtual void ApplySetOperatorChoice()
        {
            bool weHaveValidSetOp = _selectedVariable != null &&
                                    _operatorValues.Count > 0 &&
                                    _selectedOpIndex >= 0 &&
                                    _selectedOpIndex < _operatorValues.Count;
            if (weHaveValidSetOp)
            {
                SetOperator chosenOp = _operatorValues[_selectedOpIndex];
                _setOperatorProp.enumValueIndex = (int)chosenOp;
            }
        }

        protected virtual void HandleRhsValueField()
        {
            if (_selectedVariable == null)
            {
                return;
            }

            var innerDataRefProp = _anyVarDataProp != null
                ? _anyVarDataProp.FindPropertyRelative("_data")
                : null;

            if (innerDataRefProp != null && innerDataRefProp.managedReferenceValue == null)
            {
                var varType = _selectedVariable.GetType();
                Type desiredDataType = VariableDataTypeRegistry.CreateForVar(varType)?.GetType();
                if (desiredDataType != null)
                {
                    innerDataRefProp.managedReferenceValue = System.Activator.CreateInstance(desiredDataType);
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
            }

            if (_anyVarDataProp != null)
            {
                EditorGUILayout.PropertyField(_anyVarDataProp, _valueToApplyLabel, true);
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
