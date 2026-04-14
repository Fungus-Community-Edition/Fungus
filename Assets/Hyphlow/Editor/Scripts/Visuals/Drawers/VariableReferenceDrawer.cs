using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Custom drawer for VariableReference, allows selecting a target variable.
    /// Supports filtering via ContentTypeConstraint.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            UnityObj targetObject = property.serializedObject.targetObject;
            Type[] allowedContentTypes = GetAllowedTypes(fieldInfo);
            VariableRegistry varRegistry = null;

            EnsurePrerequisites(out bool canContinue);
            void EnsurePrerequisites(out bool success)
            {
                success = false;
                
                varRegistry = VariableRegistryService.Registry;
                if (varRegistry == null)
                {
                    EditorGUI.LabelField(position, label.text, "Variable registry not available.");
                    EditorGUI.EndProperty();
                    return;
                }

                success = true;
            }

            if (!canContinue)
            {
                return;
            }

            var validVarsInScene = varRegistry.GetVarsOfMultiTypes(allowedContentTypes);

            List<IVariable> candidates = validVarsInScene.Values.ToList();
            string[] options = validVarsInScene.Keys
                .Prepend("<None>")
                .ToArray();

            SerializedProperty itemIdProp = property.FindPropertyRelative("itemId");
            SerializedProperty owningSourceProp = property.FindPropertyRelative("owningSource");

            int currentItemId = itemIdProp.intValue;
            UnityObj storedOwner = owningSourceProp.objectReferenceValue;

            int currentIndex = 0;
            bool validId = currentItemId != Muscariable.InvalidID;
            if (validId)
            {
                int found = candidates.FindIndex(IsVarWithRightIdAndOwner);

                if (found >= 0)
                {
                    currentIndex = found + 1;
                }
            }

            bool IsVarWithRightIdAndOwner(IVariable varEl)
            {
                // To avoid ID collision issues, we also check that the owner of the variable
                // matches the stored owner reference. This way, even if there are multiple
                // variables with the same ID, we should still show the correct one as
                // selected in the dropdown.
                if (varEl == null)
                {
                    return false;
                }
                if (varEl.ItemId != currentItemId)
                {
                    return false;
                }
                if (storedOwner == null)
                {
                    return true;
                }
                return ReferenceEquals(varEl.Owner as UnityObj, storedOwner);
            }
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);
            // ^This is what lets the user choose a variable from the dropdown, and it returns the index of the chosen option

            if (EditorGUI.EndChangeCheck())
            {
                bool choseToSetNullVar = newIndex == 0;
                if (choseToSetNullVar)
                {
                    itemIdProp.intValue = Muscariable.InvalidID;
                    owningSourceProp.objectReferenceValue = null;
                }
                else
                {
                    IVariable chosen = candidates[newIndex - 1];
                    // ^Need the -1 because of the <None> option at index 0
                    itemIdProp.intValue = chosen.ItemId;
                    owningSourceProp.objectReferenceValue = chosen.Owner as UnityObj;
                }

                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        private static Type[] GetAllowedTypes(FieldInfo fieldInfo)
        {
            Type[] result;
            var attr = fieldInfo.GetCustomAttribute<ContentTypeConstraintAttribute>();
            if (attr != null && attr.AllowedTypes != null && attr.AllowedTypes.Count > 0)
            {
                result = attr.AllowedTypes.ToArray();
            }
            else
            {
                result = Array.Empty<Type>();
            }
            return result;
        }
    }
}