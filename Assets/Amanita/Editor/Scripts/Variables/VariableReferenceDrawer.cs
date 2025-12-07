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
            Type[] allowedTypes = GetAllowedTypes(fieldInfo);
            List<IVariable> candidates = CollectVariables(targetObject, allowedTypes);

            string[] options = candidates.Select(varEl => varEl.Key)
                .Prepend("<None>")
                .ToArray();

            SerializedProperty itemIdProp = property.FindPropertyRelative("itemId");
            int currentItemId = itemIdProp.intValue;
            int currentIndex = 0;
            if (currentItemId != 0)
            {
                int found = candidates.FindIndex(varEl => varEl.ItemId == currentItemId);
                if (found >= 0) currentIndex = found + 1;
            }

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options);

            if (newIndex == 0)
            {
                itemIdProp.intValue = 0;
            }
            else
            {
                IVariable chosen = candidates[newIndex - 1];
                itemIdProp.intValue = chosen.ItemId;
                // If you want to persist VarOwner, mark it [SerializeField] and set it here too
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
            var attr = fieldInfo.GetCustomAttribute<VarTypeConstraintAttribute>();
            if (attr != null && attr.AllowedTypes != null && attr.AllowedTypes.Count > 0)
                return attr.AllowedTypes.ToArray();
            return Array.Empty<Type>();
        }

        private static List<IVariable> CollectVariables(UnityEngine.Object context, Type[] allowedTypes)
        {
            Flowchart fChart = context as Flowchart;
            if (fChart == null)
            {
                var comp = context as Component;
                if (comp != null)
                    fChart = comp.GetComponentInParent<Flowchart>();
            }

            if (fChart == null)
            {
                return new List<IVariable>();
            }

            var vars = fChart.Variables;
            bool goForAnyType = allowedTypes.Length == 0;
            if (goForAnyType)
            {
                return vars.ToList();
            }

            List<IVariable> result = vars.Where(varEl => IsTypeAllowed(varEl, allowedTypes)).ToList();
            return result;
        }

        private static bool IsTypeAllowed(IVariable variable, Type[] allowedTypes)
        {
            if (allowedTypes.Length == 0)
            {
                return true;
            }
            Type varType = variable.GetType();
            bool result = allowedTypes.Any(typeEl => typeEl.IsAssignableFrom(varType));
            return result;  
        }
    }
}