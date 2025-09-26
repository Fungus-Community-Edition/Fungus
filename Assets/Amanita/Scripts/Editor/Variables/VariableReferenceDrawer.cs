using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for the VariableReference, allows for more easily selecting a target variable in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(VariableReference))]
    public class VariableReferenceDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property == null)
            {
                Debug.LogWarning($"VariableReferenceDrawer ONGUI has no property to work with. Exiting early.");
                return;
            }

            // If the backing object was destroyed between frames (common when running large suites)
            if (property.serializedObject == null || property.serializedObject.targetObject == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Target lost"));
                return;
            }

            var beginLabel = EditorGUI.BeginProperty(position, label, property);
            var startPos = position;
            position = EditorGUI.PrefixLabel(position, beginLabel);
            position.height = EditorGUIUtility.singleLineHeight;

            var variableProp = property.FindPropertyRelative("variable");
            if (variableProp == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Invalid VariableReference (missing 'variable')"));
                EditorGUI.EndProperty();
                return;
            }

            // Safe cast
            var v = variableProp.objectReferenceValue as Variable;

            // Auto-detect owning flowchart once
            if (variableProp.objectReferenceValue != null && lastFlowchart == null && v != null)
            {
                lastFlowchart = v.GetComponent<Flowchart>();
            }

            // Flowchart selector
            lastFlowchart = EditorGUI.ObjectField(position, lastFlowchart, typeof(Flowchart), true) as Flowchart;
            position.y += EditorGUIUtility.singleLineHeight;

            if (lastFlowchart != null)
            {
                var popupRect = startPos;
                popupRect.y = position.y;
                var prefixLabel = new GUIContent(v != null ? v.GetType().Name : "No Var Selected");
                EditorGUI.indentLevel++;
                VariableEditor.VariableField(
                    variableProp,
                    prefixLabel,
                    lastFlowchart,
                    "<None>",
                    null,
                    (popupLabel, selectedIndex, displayedOptions) =>
                        EditorGUI.Popup(popupRect, popupLabel, selectedIndex, displayedOptions)
                );
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.PrefixLabel(position, new GUIContent("Flowchart Required"));
            }

            // Commit changes defensively
            variableProp.serializedObject?.ApplyModifiedProperties();
            property.serializedObject?.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Preserve existing layout height; guard against null for robustness
            return EditorGUIUtility.singleLineHeight * 2f;
        }
    }
}