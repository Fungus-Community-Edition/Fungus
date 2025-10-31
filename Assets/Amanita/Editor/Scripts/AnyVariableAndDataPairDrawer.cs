using System;
using System.Collections.Generic;
using System.Linq;
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
                SerializedProperty anyVarDataProp = holdsVarAndDataPair.FindPropertyRelative("data");
                AnyVariableData anyVarData = anyVarDataProp?.managedReferenceValue as AnyVariableData;

                if (anyVarData != null && currentLeftHandSideVar != null)
                {
                    anyVarData.SetFor(currentLeftHandSideVar.GetType(), currentLeftHandSideVar.ContentType);
                }
                // ^No need to only execute this when lhs var changes, since SetFor already
                // checks internally
                HandleLhsVarChanges();
                void HandleLhsVarChanges()
                {
                    bool lhsVarChanged = !ReferenceEquals(_prevLeftHandSideVar, currentLeftHandSideVar);
                    bool validAnyVarData = anyVarData != null;
                    if (lhsVarChanged && validAnyVarData && currentLeftHandSideVar != null)
                    {
                        // When currentLeftHandSideVar is null, we don't want to change the var type
                        // of the inner data field. Later in this func, we'll just make sure
                        // not to render it
                        Debug.Log($"Updating the var type of the rhs");
                        _prevLeftHandSideVar = currentLeftHandSideVar;
                    }
                }

                holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();

                DrawInnerDataField();
                void DrawInnerDataField()
                {
                    SerializedProperty innerDataProp = holdsVarAndDataPair.FindPropertyRelative("data.data");
                    // ^Expected to hold a VariableData subclass as its boxed and object ref values

                    if (currentLeftHandSideVar != null && innerDataProp != null)
                    {
                        // Avoid Unity trying to instantiate a generic drawer (VariableDataDrawer`1[T])
                        // by drawing the managed reference's children directly.
                        if (innerDataProp.propertyType == SerializedPropertyType.ManagedReference &&
                            !string.IsNullOrEmpty(innerDataProp.managedReferenceFullTypename))
                        {
                            EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"), includeChildren: true);
                        }
                        else
                        {
                            // Fallback: let Unity draw if it is not a managed reference
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
            return prop.objectReferenceValue as IVariable;
        }

        protected IVariable _prevLeftHandSideVar;

        protected static bool TryGetTypeActionsFor(System.Type varPropType, out VariableTypeActions typeActionsRes)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varPropType, out typeActionsRes);
        }

    }
}