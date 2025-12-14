using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for VariableReference, allows selecting a target variable.
    /// Supports filtering via VarTypeConstraint.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            UnityObj targetObject = property.serializedObject.targetObject;
            Type[] allowedContentTypes = GetAllowedTypes(fieldInfo);

            var ammieManager = AmanitaManager.S;
            if (ammieManager == null)
            {
                EditorGUI.LabelField(position, label.text, "AmanitaManager not found in scene.");
                EditorGUI.EndProperty();
                return;
            }

            var varRegistry = ammieManager.VariableRegistry;
            var validVarsInScene = varRegistry.GetVarsOfMultiTypes(allowedContentTypes);
            
            List<IVariable> candidates = validVarsInScene.Values.ToList();
            string[] options = validVarsInScene.Keys
                .Prepend("<None>")
                .ToArray();

            SerializedProperty itemIdProp = property.FindPropertyRelative("itemId");
            int currentItemId = itemIdProp.intValue;
            int currentIndex = 0;
            if (currentItemId != Muscariable.InvalidID)
            {
                int found = candidates.FindIndex(varEl => varEl.ItemId == currentItemId);
                if (found >= 0)
                {
                    currentIndex = found + 1;
                }
            }
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

            if (newIndex == 0)
            {
                itemIdProp.intValue = Muscariable.InvalidID;
            }
            else
            {
                IVariable chosen = candidates[newIndex - 1];
                // ^Need the -1 because of the <None> option at index 0
                itemIdProp.intValue = chosen.ItemId;
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