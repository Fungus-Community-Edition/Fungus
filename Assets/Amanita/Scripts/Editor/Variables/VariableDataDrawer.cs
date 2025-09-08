using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    public class VariableDataDrawer<T> : PropertyDrawer where T : Variable
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SafeIMGUI.Draw(() =>
            {
                EditorGUI.BeginProperty(position, label, property);

                VariableInfoAttribute attr;
                var typeInfo = VariableEditor.GetVariableInfo(typeof(T));
                if (typeInfo == null)
                {
                    EditorGUI.LabelField(position, label.text, "No VariableInfoAttribute");
                    return;
                }

                string propNameBase = char.ToLowerInvariant(typeInfo.OptionDisplayName[0]) + typeInfo.OptionDisplayName.Substring(1);
                string refPropName = propNameBase + "Ref"; // Example: integerRef

                // Reference and literal lookups with compatibility fallbacks
                var referenceProp = property.FindPropertyRelative(refPropName);
                var valueProp = FindLiteralProp(property, propNameBase);

                // If both are missing, show a small warning but don’t break the GUI
                if (referenceProp == null && valueProp == null)
                {
                    EditorGUI.LabelField(position, label.text, "Invalid variable data fields");
                    return;
                }

                // If the reference slot exists, draw per the original UX:
                // - If ref is null: show literal + compact ref popup
                // - If ref is set: show just the ref field
                var flowchart = (property.serializedObject.targetObject as Command)?.GetFlowchart();
                if (flowchart == null || flowchart.VariableCount == 0)
                {
                    EditorGUI.LabelField(noContentFoundRect, "No Flowchart or Variables found");
                    return;
                }

                // Decide layout based on value property height (if we have one)
                float valueHeight = valueProp != null
                    ? EditorGUI.GetPropertyHeight(valueProp, label)
                    : EditorGUIUtility.singleLineHeight;

                if (valueHeight <= EditorGUIUtility.singleLineHeight * 2f)
                {
                    DrawSingleLine(position, label, referenceProp, valueProp);
                }
                else
                {
                    DrawMultiLine(position, label, referenceProp, valueProp);
                }

            }, property.displayName);

        }

        protected static Rect noContentFoundRect = new Rect(0, 0, 100, 20);

        // Compatibility finder for literal value fields across legacy/new layouts
        private static SerializedProperty FindLiteralProp(SerializedProperty root, string baseName)
        {
            // 1) Legacy explicit value naming: floatVal, vector3Val, etc.
            string legacyValueName = baseName + "Val";
            SerializedProperty propFound = root.FindPropertyRelative(legacyValueName);
            if (propFound != null) return propFound;

            // 2) New generic field in VariableData<T>
            propFound = root.FindPropertyRelative("_valOfType");
            if (propFound != null) return propFound;

            // 3) Very old generic ‘value’ naming in some data types
            propFound = root.FindPropertyRelative("value");
            if (propFound != null) return propFound;

            // 4) Base class object fallback (valObj). This is the last resort
            propFound = root.FindPropertyRelative("valObj");

            return propFound;
        }

        private static void DrawSingleLine(Rect rect, GUIContent label, SerializedProperty referenceProp,
            SerializedProperty valueProp)
        {
            // If there’s no reference field at all, just draw the literal
            if (referenceProp == null)
            {
                EditorGUI.PropertyField(rect, valueProp ?? referenceProp, label, true);
                return;
            }

            int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
            const int popupGap = 5;

            Rect controlRect = EditorGUI.PrefixLabel(rect, label);
            Rect valueRect = controlRect;
            valueRect.width = Mathf.Max(0, controlRect.width - popupWidth - popupGap);
            Rect popupRect = controlRect;

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            bool shouldDrawLiteral = referenceProp.objectReferenceValue == null && valueProp != null;
            if (shouldDrawLiteral)
            {
                CustomVariableDrawerLookup.DrawCustomOrPropertyField(typeof(T), valueRect, valueProp, GUIContent.none);
                popupRect.x += valueRect.width + popupGap;
                popupRect.width = popupWidth;
            }

            EditorGUI.PropertyField(popupRect, referenceProp, GUIContent.none);
            EditorGUI.indentLevel = prevIndent;
        }

        private static void DrawMultiLine(Rect rect, GUIContent label, SerializedProperty referenceProp, SerializedProperty valueProp)
        {
            // If there’s no reference field at all, just draw the literal with label
            if (referenceProp == null)
            {
                EditorGUI.PropertyField(rect, valueProp ?? referenceProp, label, true);
                return;
            }

            const int popupWidth = 100;
            Rect popupRect;

            if (referenceProp.objectReferenceValue == null && valueProp != null)
            {
                CustomVariableDrawerLookup.DrawCustomOrPropertyField(typeof(T), rect, valueProp, label);
                Vector2 popupRectPos = new Vector2(rect.x + rect.width - popupWidth + 5, rect.y);
                Vector2 popupRectSize = new Vector2(popupWidth, EditorGUIUtility.singleLineHeight);
                popupRect = new Rect(popupRectPos, popupRectSize);
            }
            else
            {
                popupRect = EditorGUI.PrefixLabel(rect, label);
            }

            EditorGUI.PropertyField(popupRect, referenceProp, GUIContent.none);
        }

    }

    [CustomPropertyDrawer(typeof(BooleanData))]
    public class BooleanDataDrawer : VariableDataDrawer<BooleanVariable>
    { }

    [CustomPropertyDrawer(typeof(IntegerData))]
    public class IntegerDataDrawer : VariableDataDrawer<IntegerVariable>
    { }

    [CustomPropertyDrawer(typeof(FloatData))]
    public class FloatDataDrawer : VariableDataDrawer<FloatVariable>
    { }

    [CustomPropertyDrawer(typeof(StringData))]
    public class StringDataDrawer : VariableDataDrawer<StringVariable>
    { }

    [CustomPropertyDrawer(typeof(StringDataMulti))]
    public class StringDataMultiDrawer : VariableDataDrawer<StringVariable>
    { }
}