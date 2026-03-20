using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Amanita.VScripting.EditorUtils
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
            AmanitaManager ammieManager = null;
            VariableRegistry varRegistry = null;

            EnsurePrerequisites(out bool canContinue);
            void EnsurePrerequisites(out bool success)
            {
                success = false;

                ammieManager = AmanitaManager.S;
                if (ammieManager == null)
                {
                    EditorGUI.LabelField(position, label.text, "AmanitaManager not found in scene.");
                    EditorGUI.EndProperty();
                    return;
                }

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
            SerializedProperty owningFcProp = property.FindPropertyRelative("owningFc");
            SerializedProperty owningVsaProp = property.FindPropertyRelative("owningVsa");

            int currentItemId = itemIdProp.intValue;
            int currentIndex = 0;
            bool validId = currentItemId != Muscariable.InvalidID;
            if (validId)
            {
                int found = candidates.FindIndex(varEl => varEl.ItemId == currentItemId);
                if (found >= 0)
                {
                    currentIndex = found + 1;
                }
            }

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

            bool choseToSetNullVar = newIndex == 0;
            if (choseToSetNullVar)
            {
                itemIdProp.intValue = Muscariable.InvalidID;
                owningFcProp.objectReferenceValue = null;
                owningVsaProp.objectReferenceValue = null;
            }
            else
            {
                IVariable chosen = candidates[newIndex - 1];
                // ^Need the -1 because of the <None> option at index 0
                itemIdProp.intValue = chosen.ItemId;

                // We're not assigning through the Variable property of VariableReference, and thus
                // we have to assign the owner ourselves.
                var chosenOwner = chosen.Owner;
                owningFcProp.objectReferenceValue = chosenOwner as Flowchart;
                owningVsaProp.objectReferenceValue = chosenOwner as VariableSourceAsset;
            }

            property.serializedObject.ApplyModifiedProperties();
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