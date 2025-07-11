using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Amanita.VScripting;
using UnityObject = UnityEngine.Object;
using Type = System.Type;

namespace Amanita.EditorUtils
{
    public static class UitkFieldGenerator
    {
        public static VisualElement GenerateObjectField(IVariable varToRepresent)
        {
            var varType = varToRepresent.ContentType;

            // Handle all UnityObject variables (including AudioClip), even when Value is null
            if (typeof(UnityObject).IsAssignableFrom(varType))
            {
                return GenUnityObjectField(varToRepresent, varType);
            }

            Debug.LogWarning($"Could not generate object field for variable of type {varType.Name}");
            return null;
        }

        // Generic UnityEngine.Object field generator (works for AudioClip, Texture2D, Material, etc.)
        private static VisualElement GenUnityObjectField(IVariable variable, Type objectType)
        {
            var unityVar = variable as UnityObject;
            if (unityVar == null)
            {
                Debug.LogWarning($"Variable {variable?.GetType().Name} is not a UnityEngine.Object; cannot serialize in Inspector.");
                return null;
            }

            // Wrap the variable itself
            var so = new SerializedObject(unityVar);

            // Try to find the backing serialized field (value/baseVal/etc.)
            SerializedProperty valProp = FindValueProperty(so);

            // Current value can be null; that's fine
            var current = variable.Value as UnityObject;

            var field = new ObjectField
            {
                objectType = objectType,
                value = current
            };

            field.RegisterValueChangedCallback(evt =>
            {
                var newObj = evt.newValue as UnityObject;

                Undo.RecordObject(unityVar, $"Change {objectType.Name} Variable Value");

                if (valProp != null && valProp.propertyType == SerializedPropertyType.ObjectReference)
                {
                    // Re-resolve each time to avoid stale handles after domain reloads/rebinds
                    var liveSo = new SerializedObject(unityVar);
                    var liveProp = FindValueProperty(liveSo);

                    if (liveProp != null)
                    {
                        liveSo.Update();
                        liveProp.objectReferenceValue = newObj;
                        liveSo.ApplyModifiedProperties();
                    }
                    else
                    {
                        // Fallback if the property wasn’t found this time
                        variable.Value = newObj;
                    }
                }
                else
                {
                    // Safe fallback when we can’t find a SerializedProperty
                    variable.Value = newObj;
                }

                EditorUtility.SetDirty(unityVar);
            });

            return field;
        }

        // Heuristic search for the serialized backing field of the variable's value
        private static SerializedProperty FindValueProperty(SerializedObject so)
        {
            // Common candidates — adjust this list to match your base variable class
            string[] candidates = { "value", "baseVal", "baseValue", "m_Value" };
            foreach (var name in candidates)
            {
                var p = so.FindProperty(name);
                if (p != null) return p;
            }

            // Fallback: first object reference (not m_Script)
            var it = so.GetIterator();
            bool enterChildren = true;
            while (it.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.name != "m_Script")
                {
                    return it.Copy();
                }
            }
            return null;
        }
    }
}