using System;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for the AnyVaraibleAndDataPair, shows only the matching data for the targeted variable
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnyVariableAndDataPair))]
    public class AnyVariableAndDataPairDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty holdsVarAndDataPair, GUIContent label)
        {
            SerializedProperty lhsVarRefProp;
            DisplayLeftHandSideVar();
            void DisplayLeftHandSideVar()
            {
                lhsVarRefProp = holdsVarAndDataPair.FindPropertyRelative("varRef");
                EditorGUI.PropertyField(position, lhsVarRefProp, label);
                lhsVarRefProp.serializedObject.ApplyModifiedProperties();
            }
            IVariable currentLeftHandSideVar = ReadIVariable(lhsVarRefProp);

            AnyVariableAndDataPair pairInstance = holdsVarAndDataPair.boxedValue as AnyVariableAndDataPair;
            position.y += EditorGUIUtility.singleLineHeight;

            HandleInnerDataField();
            void HandleInnerDataField()
            {
                // Safely read AnyVariableData whether Unity reports ManagedReference or Generic.
                SerializedProperty anyVarDataProp = holdsVarAndDataPair.FindPropertyRelative("data");
                AnyVariableData anyVarData = anyVarDataProp.boxedValue as AnyVariableData;

                if (anyVarData != null && currentLeftHandSideVar != null)
                {
                    var effectiveVarType = GetEffectiveVarType(currentLeftHandSideVar);
                    anyVarData.SetFor(effectiveVarType, currentLeftHandSideVar.ContentType);
                }

                HandleLhsVarChanges();
                void HandleLhsVarChanges()
                {
                    bool lhsVarChanged = !ReferenceEquals(_prevLeftHandSideVar, currentLeftHandSideVar);
                    bool validAnyVarData = anyVarData != null;
                    if (lhsVarChanged && validAnyVarData && currentLeftHandSideVar != null)
                    {
                        _prevLeftHandSideVar = currentLeftHandSideVar;
                        pairInstance.LhsVariable = currentLeftHandSideVar;
                    }
                }

                anyVarDataProp.boxedValue = anyVarData;
                holdsVarAndDataPair.boxedValue = pairInstance;
                holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();

                DrawInnerDataField();
                void DrawInnerDataField()
                {
                    SerializedProperty innerDataProp = holdsVarAndDataPair.FindPropertyRelative("data.data");
                    if (currentLeftHandSideVar != null && innerDataProp != null)
                    {
                        EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"), includeChildren: true);
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Must select a variable before setting data.");
                    }
                }
            }

            GUILayout.Space(20);
            holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();
        }

        private static IVariable ReadIVariable(SerializedProperty prop)
        {
            // We assume that we are drawing as part of a Command's editor fields, and that
            // thus we have a Flowchart selected. We'll use that to find the variable instance.
            VariableReference reference = (VariableReference)prop.boxedValue;
            reference.VarOwner = FlowchartWindow.GetFlowchart();
            return reference.Variable;
        }

        private static Type GetEffectiveVarType(IVariable var)
        {
            if (var is IVariablePointer ptr && ptr.Component is IVariable inner)
                return inner.GetType();
            return var?.GetType();
        }

        protected IVariable _prevLeftHandSideVar;

        protected static bool TryGetTypeActionsFor(System.Type varPropType, out VariableTypeActions typeActionsRes)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varPropType, out typeActionsRes);
        }
    }
}