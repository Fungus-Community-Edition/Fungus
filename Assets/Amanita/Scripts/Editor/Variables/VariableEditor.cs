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
            RegisterVarsToShowInDropdown();
            void RegisterVarsToShowInDropdown()
            {
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

    // For drawing fields in Commands that should ONLY accept variable inputs
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
                        typeListToCheck = VariableTypeRegistry.AllLegacyTypes;
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

    
}
