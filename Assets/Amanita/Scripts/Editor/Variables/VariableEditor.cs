using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor (typeof(Variable), true)]
    public class VariableEditor : CommandEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();

            Variable varTarget = target as Variable;
            varTarget.hideFlags = HideFlags.HideInInspector;
        }

        public static VariableInfoAttribute GetVariableInfo(System.Type variableType)
        {
            object[] attributes = variableType.GetCustomAttributes(typeof(VariableInfoAttribute), false);
            foreach (object obj in attributes)
            {
                VariableInfoAttribute variableInfoAttr = obj as VariableInfoAttribute;
                if (variableInfoAttr != null)
                {
                    return variableInfoAttr;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Handles drawing the dropdown that lets you select variables in a Command's UI.
        /// Filter decides if a particular variable should be an option in the dropdown.
        /// </summary>
        public static void VariableField(SerializedProperty property, 
                                         GUIContent label, 
                                         Flowchart flowchart,
                                         string defaultText,
                                         Func<Variable, bool> filter, 
                                         Func<string, int, string[], int> drawer = null)
        {
            List<string> variableKeys = new List<string>() { defaultText };
            List<Variable> variableObjects = new List<Variable>() { null };

            IList<IVariable> variables = flowchart.Variables;
            int index = 0;
            int selectedIndex = 0;

            Variable selectedVariable = property.objectReferenceValue as Variable;

            AvoidGlitchInvolvingFlowchartSwitches();
            void AvoidGlitchInvolvingFlowchartSwitches()
            {
                // When there are multiple Flowcharts in a scene with variables, switching
                // between the Flowcharts can cause the wrong variable property
                // to be inspected for a single frame. This has the effect of causing private
                // variable references to be set to null when inspected. When this condition 
                // occurs we just skip displaying the property for this frame.
                if (selectedVariable != null &&
                    selectedVariable.gameObject != flowchart.gameObject &&
                    selectedVariable.Scope == VariableScope.Private)
                {
                    property.objectReferenceValue = null;
                    return;
                }
            }

            foreach (Variable elem in variables)
            {
                if (filter != null && !filter(elem))
                {
                    continue;
                }
                
                variableKeys.Add(elem.Key);
                variableObjects.Add(elem);
                
                index++;
                
                if (elem == selectedVariable)
                {
                    selectedIndex = index;
                }
            }

            List<Flowchart> fcList = Flowchart.CachedFlowcharts;
            foreach (Flowchart fcElem in fcList)
            {
                if (fcElem == flowchart)
                {
                    continue;
                }

                List<Variable> publicVars = fcElem.GetPublicVariables();
                foreach (Variable varElem in publicVars)
                {
                    if (filter != null)
                    {
                        if (!filter(varElem))
                        {
                            continue;
                        }
                    }

                    variableKeys.Add(fcElem.name + "/" + varElem.Key);
                    variableObjects.Add(varElem);

                    index++;

                    if (varElem == selectedVariable)
                    {
                        selectedIndex = index;
                    }
                }
            }

            if (drawer == null)
            {
                selectedIndex = EditorGUILayout.Popup(label.text, selectedIndex, variableKeys.ToArray());
            }
            else
            {
                selectedIndex = drawer(label.text, selectedIndex, variableKeys.ToArray());
            }

            property.objectReferenceValue = variableObjects[selectedIndex];
        }
    }

    [CustomPropertyDrawer(typeof(VariablePropertyAttribute))]
    public class VariableDrawer : PropertyDrawer
    {   
        public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
        {

            VariablePropertyAttribute variableProperty = attribute as VariablePropertyAttribute;
            if (variableProperty == null)
            {
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // Filter the variables by the types listed in the VariableProperty attribute
            Func<Variable, bool> compare = varToCheck => 
            {
                var varType = varToCheck.GetType();
                if (varToCheck == null)
                {
                    return false;
                } 

                if (variableProperty.VariableTypes.Length == 0)
                {
                    // Use VariableTypeRegistry.AllTypes for filtering
                    var allTypes = VariableTypeRegistry.AllTypes;
                    bool result = allTypes.Any((typeInColl) => typeInColl.Equals(varType));
                    return result;
                }

                return variableProperty.VariableTypes.Contains<System.Type>(varType);
            };

            VariableEditor.VariableField(property, 
                                         label,
                                         FlowchartWindow.GetFlowchart(),
                                         variableProperty.defaultText,
                                         compare,
                                         (label, selectedIndex, optionsToDisplay) => 
                                         (EditorGUI.Popup(position, label, selectedIndex, optionsToDisplay)));

            EditorGUI.EndProperty();
        }
    }

    public class VariableDataDrawer<T> : PropertyDrawer where T : Variable
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SafeIMGUI.Draw(() =>
            {
                EditorGUI.BeginProperty(position, label, property);
                var typeInfo = VariableEditor.GetVariableInfo(typeof(T));
                if (typeInfo == null)
                {
                    EditorGUI.LabelField(position, label.text, "No VariableInfoAttribute");
                    return;
                }

                string propNameBase = char.ToLowerInvariant(typeInfo.VariableType[0]) + typeInfo.VariableType.Substring(1);
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
                if (flowchart == null || flowchart.Variables == null)
                {
                    EditorGUI.LabelField(new Rect(0,0,100,20), "No Flowchart or Variables found");
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

            if (referenceProp.objectReferenceValue == null && valueProp != null)
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

    [CustomPropertyDrawer (typeof(BooleanData))]
    public class BooleanDataDrawer : VariableDataDrawer<BooleanVariable>
    {}

    [CustomPropertyDrawer (typeof(IntegerData))]
    public class IntegerDataDrawer : VariableDataDrawer<IntegerVariable>
    {}

    [CustomPropertyDrawer (typeof(FloatData))]
    public class FloatDataDrawer : VariableDataDrawer<FloatVariable>
    {}

    [CustomPropertyDrawer (typeof(StringData))]
    public class StringDataDrawer : VariableDataDrawer<StringVariable>
    {}

    [CustomPropertyDrawer (typeof(StringDataMulti))]
    public class StringDataMultiDrawer : VariableDataDrawer<StringVariable>
    {}
}
