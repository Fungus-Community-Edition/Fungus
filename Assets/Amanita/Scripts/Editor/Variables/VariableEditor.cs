using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityObject = UnityEngine.Object;

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
        /// </summary>
        public static void VariableField(SerializedProperty property, 
                                         GUIContent label, 
                                         Flowchart flowchartBelongingToCommand,
                                         string defaultText,
                                         Func<IVariable, bool> shouldBeOptionInDropdown, 
                                         Func<string, int, string[], int> drawer = null)
        {
            bool allowAnythingInSelectionList = shouldBeOptionInDropdown == null;
            if (allowAnythingInSelectionList)
            {
                shouldBeOptionInDropdown = (varInQuestion) => true;
            }

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
                    selectedVariable.gameObject != flowchartBelongingToCommand.gameObject &&
                    selectedVariable.Scope == VariableScope.Private)
                {
                    property.objectReferenceValue = null;
                    return;
                }
            }

            IReadOnlyList<IVariable> varsToCheck = flowchartBelongingToCommand.Variables;
            int index = 0;
            int selectedIndex = 0;
            IList<string> variableKeys = new List<string>() { defaultText };
            IList<IVariable> variableObjects = new List<IVariable>() { null };
            RegisterLocalVarsToShowInDropdown();
            void RegisterLocalVarsToShowInDropdown()
            {
                // As in local to the Flowchart the Command belongs to
                for (int i = 0; i < varsToCheck.Count; i++)
                {
                    var elem = varsToCheck[i];
                    if (!shouldBeOptionInDropdown(elem))
                    {
                        continue;
                    }

                    variableKeys.Add(elem.Key);
                    variableObjects.Add(elem);
                    index++;

                    // Given the nature of Unity's serialization system, we'll assume that 
                    // all IVariables here are in UnityObject's family tree. We'll probably
                    // want to use an editor-only holder of sorts for Muscariables when
                    // we get around to integrating those.
                    if ((UnityObject)elem == selectedVariable)
                    {
                        selectedIndex = index;
                    }
                }
            }

            // We want the appropriate public variables of other Flowcharts in the scene
            // to be selectable as well. Thus, we'll scan those too.
            RegisterOtherPublicVarsToShowInDropdown();
            void RegisterOtherPublicVarsToShowInDropdown()
            {
                List<Flowchart> fcList = Flowchart.CachedFlowcharts;

                for (int fcListIndex = 0; fcListIndex < fcList.Count; fcListIndex++)
                {
                    Flowchart fcElem = fcList[fcListIndex];
                    if (fcElem == flowchartBelongingToCommand)
                    {
                        continue;
                    }

                    IList<IVariable> publicVars = fcElem.GetPublicVariables();
                    for (int publicVarIndex = 0; publicVarIndex < publicVars.Count; publicVarIndex++)
                    { 
                        IVariable varElem = publicVars[publicVarIndex];
                        if (!shouldBeOptionInDropdown(varElem))
                        {
                            continue;
                        }

                        string publicVarKey = $"{fcElem.name}/{varElem.Key}";
                        // ^To make it easy to see that the var belongs to another
                        // flowchart
                        variableKeys.Add(publicVarKey);
                        variableObjects.Add(varElem);

                        index++;

                        if ((UnityObject)varElem == selectedVariable)
                        {
                            selectedIndex = index;
                        }
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

            if (selectedIndex == 0)
            {
                property.objectReferenceValue = (UnityObject)variableObjects[selectedIndex];
            }
            else
            {
                property.objectReferenceValue = (UnityObject)variableObjects[selectedIndex];
            }
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

            bool ShouldBeAnOptionInTheDropdown(IVariable varToCheck)
            {
                // ^We decide this based on whether the var to check is of a type that is included
                // in the varProp's type list. 
                bool whetherItDoesOrNot = false;

                if (varToCheck != null)
                {
                    var typeToCheck = varToCheck.GetType();

                    IReadOnlyList<Type> typeListToCheck;
                    bool shouldCheckForAllTypes = variableProperty.VariableTypes.Length == 0;
                    // ^Though for flexibility's sake, we made it so that having no types in the 
                    // prop's list means we should list any var of any type in the registry
                    if (shouldCheckForAllTypes)
                    {
                        typeListToCheck = VariableTypeRegistry.AllTypes;
                    }
                    else
                    {
                        typeListToCheck = variableProperty.VariableTypes;
                    }

                    whetherItDoesOrNot = typeListToCheck.Any((typeInList) => typeInList.Equals(typeToCheck));
                }

                return whetherItDoesOrNot;
                
            }

            VariableEditor.VariableField(property, 
                                         label,
                                         FlowchartWindow.GetFlowchart(),
                                         variableProperty.defaultText,
                                         ShouldBeAnOptionInTheDropdown,
                                         VariableSelectionPopup);

            // Returns the index of the option selected
            int VariableSelectionPopup(string label,  int selectedIndex, string[] optionsToDisplay)
            {
                return EditorGUI.Popup(position, label, selectedIndex, optionsToDisplay);
            }

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
