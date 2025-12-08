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
            SerializedProperty leftHandSideVarProp;
            DisplayLeftHandSideVar();
            void DisplayLeftHandSideVar()
            {
                leftHandSideVarProp = holdsVarAndDataPair.FindPropertyRelative("variable");
                // Draw using the VariableDrawer (handles both ObjectReference and ManagedReference)
                EditorGUI.PropertyField(position, leftHandSideVarProp, label);
            }

            position.y += EditorGUIUtility.singleLineHeight;

            HandleInnerDataField();
            void HandleInnerDataField()
            {
                // Read IVariable correctly based on property type
                IVariable currentLeftHandSideVar = ReadIVariable(leftHandSideVarProp);

                // Safely read AnyVariableData whether Unity reports ManagedReference or Generic.
                SerializedProperty anyVarDataProp = holdsVarAndDataPair.FindPropertyRelative("data");
                AnyVariableData anyVarData = null;
                if (anyVarDataProp != null)
                {
                    if (anyVarDataProp.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        anyVarData = anyVarDataProp.managedReferenceValue as AnyVariableData;
                    }
                    else if (anyVarDataProp.propertyType == SerializedPropertyType.Generic)
                    {
                        anyVarData = anyVarDataProp.boxedValue as AnyVariableData;
                    }
                }
                //
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
                    }
                }

                holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();

                DrawInnerDataField();
                void DrawInnerDataField()
                {
                    SerializedProperty innerDataProp = holdsVarAndDataPair.FindPropertyRelative("data.data");
                    if (currentLeftHandSideVar != null && innerDataProp != null)
                    {
                        if (innerDataProp.propertyType == SerializedPropertyType.ManagedReference &&
                            !string.IsNullOrEmpty(innerDataProp.managedReferenceFullTypename))
                        {
                            EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"), includeChildren: true);
                        }
                        else
                        {
                            EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"), includeChildren: true);
                        }
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

        // Read IVariable for both ObjectReference and ManagedReference fields
        private static IVariable ReadIVariable(SerializedProperty prop)
        {
            if (prop == null) return null;
            if (prop.propertyType == SerializedPropertyType.ManagedReference)
            {
                return prop.managedReferenceValue as IVariable;
            }
            if (prop.propertyType == SerializedPropertyType.Generic)
            {
                return prop.boxedValue as IVariable;
            }
            return prop.objectReferenceValue as IVariable;
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