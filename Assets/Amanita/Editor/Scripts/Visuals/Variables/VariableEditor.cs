using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityObject = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
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

        public static VariableInfoAttribute GetVariableInfo(Type variableType)
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
        /// Works for both ObjectReference-backed (legacy Variable) and ManagedReference-backed (IVariable) properties.
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

            // Property may be an ObjectReference (MonoBehaviour/ScriptableObject) or a ManagedReference (IVariable)
            bool isManagedRef = property.propertyType == SerializedPropertyType.ManagedReference;

            Variable selectedLegacy = null;
            IVariable selectedIVar = null;

            if (!isManagedRef)
            {
                selectedLegacy = property.objectReferenceValue as Variable;
            }
            else
            {
                selectedIVar = property.managedReferenceValue as IVariable;
            }

            AvoidGlitchInvolvingFlowchartSwitches();
            void AvoidGlitchInvolvingFlowchartSwitches()
            {
                // Only applies to legacy Variables (MonoBehaviours) which can belong to another Flowchart
                if (!isManagedRef && selectedLegacy != null &&
                    flowchartBelongingToCommand != null &&
                    selectedLegacy.gameObject != flowchartBelongingToCommand.gameObject &&
                    selectedLegacy.Scope == VariableScope.Private)
                {
                    property.objectReferenceValue = null;
                    selectedLegacy = null;
                    return;
                }
            }

            IReadOnlyList<IVariable> varsToCheck = flowchartBelongingToCommand != null
                                                   ? flowchartBelongingToCommand.Variables
                                                   : Array.Empty<IVariable>();
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

                        // Selection match logic:
                        // - If property is ObjectReference: match against legacy Variable or any UnityObject implementing IVariable.
                        // - If ManagedReference: semantically match managed or legacy via pointer unwrapping.
                        var elemAsUnityObj = elem as UnityObject;
                        if (!isManagedRef && selectedLegacy != null && elemAsUnityObj == selectedLegacy)
                        {
                            selectedIndex = index;
                        }
                        else if (isManagedRef && selectedIVar != null && VarsSemanticallyEqual(selectedIVar, elem))
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
                    IReadOnlyList<Flowchart> fcList = FlowchartRegistry.GetFlowcharts();

                    for (int fcListIndex = 0; fcListIndex < fcList.Count; fcListIndex++)
                    {
                        Flowchart fcElem = fcList[fcListIndex];
                        if (fcElem == flowchartBelongingToCommand)
                        {
                            continue;
                        }

                        IList<IVariable> publicVars = fcElem.Variables.Where(IsVarPublic).ToList();
                        for (int publicVarIndex = 0; publicVarIndex < publicVars.Count; publicVarIndex++)
                        {
                            IVariable varElem = publicVars[publicVarIndex];
                            if (!shouldBeOptionInDropdown(varElem))
                            {
                                continue;
                            }

                            string publicVarKey = $"{fcElem.name}/{varElem.Key}";
                            // ^To make it clear which vars belong to which Flowcharts

                            variableKeys.Add(publicVarKey);
                            variableObjects.Add(varElem);
                            index++;

                            var elemAsUnityObj = varElem as UnityObject;
                            if (!isManagedRef && selectedLegacy != null && elemAsUnityObj == selectedLegacy)
                            {
                                selectedIndex = index;
                            }
                            else if (isManagedRef && selectedIVar != null && VarsSemanticallyEqual(selectedIVar, varElem))
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

            // Apply selection to the property
            IVariable chosen = variableObjects[selectedIndex];
            if (isManagedRef)
            {
                if (chosen == null)
                {
                    property.managedReferenceValue = null;
                }
                else
                {
                    // If the destination is AnyVariableAndDataPair.variable and the choice is legacy,
                    // route to the sibling legacyVariable field instead of writing a UnityObj into a SerializeReference.
                    var chosenAsUnityObj = chosen as UnityObject;
                    if (chosenAsUnityObj != null)
                    {
                        // Try to detect the AnyVariableAndDataPair.variable path and set its legacyVariable sibling.
                        string legacyPath = ComputeSiblingLegacyPath(property.propertyPath);
                        if (!string.IsNullOrEmpty(legacyPath))
                        {
                            var legacyProp = property.serializedObject.FindProperty(legacyPath);
                            if (legacyProp != null && legacyProp.propertyType == SerializedPropertyType.ObjectReference)
                            {
                                legacyProp.objectReferenceValue = chosenAsUnityObj as Variable;
                                property.managedReferenceValue = null; // keep only one authoritative source
                                // Apply so subsequent GUI draws are in sync
                                legacyProp.serializedObject.ApplyModifiedProperties();
                                property.serializedObject.ApplyModifiedProperties();
                            }
                            else
                            {
                                // Fallback: if not our pair, assign as before to managed ref (for other systems)
                                property.managedReferenceValue = chosen;
                            }
                        }
                        else
                        {
                            // Not an AnyVariableAndDataPair.variable – leave behavior unchanged
                            property.managedReferenceValue = chosen;
                        }
                    }
                    else
                    {
                        // Pure managed IVariable is safe to assign directly
                        property.managedReferenceValue = chosen;
                    }
                }
            }
            else
            {
                // For ObjectReference fields, only UnityEngine.Object-backed variables can be assigned
                property.objectReferenceValue = chosen as UnityObject;
            }
        }

        private static bool IsVarPublic(IVariable elem)
        {
            return elem.Scope == VariableScope.Public;
        }

        private static string ComputeSiblingLegacyPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            // Expect last segment 'variable' => replace with 'legacyVariable'
            // Works for nested paths like "pairs.Array.data[0].variable"
            int idx = path.LastIndexOf(".variable", StringComparison.Ordinal);
            if (idx >= 0)
            {
                return path.Substring(0, idx) + ".legacyVariable";
            }
            // Root field named 'variable'
            if (path == "variable")
                return "legacyVariable";

            return null;
        }

        private static bool VarsSemanticallyEqual(IVariable first, IVariable second)
        {
            if (first == null || second == null) return false;

            // Unwrap pointers if needed
            IVariable Unwrap(IVariable v)
            {
                if (v is IVariablePointer p && p.Component is IVariable inner) return inner;
                return v;
            }
            first = Unwrap(first);
            second = Unwrap(second);

            try
            {
                if ((first.ItemId != 0 || second.ItemId != 0) && first.ItemId == second.ItemId) return true;
            }
            catch { /* ignore */ }

            try
            {
                bool bothHaveValidOwners = first.Owner != null && second.Owner != null;
                bool bothHaveSameValidOwner = bothHaveValidOwners && ReferenceEquals(first.Owner, second.Owner);
                bool bothHaveSameValidKey = !string.IsNullOrEmpty(first.Key) && first.Key == second.Key;
                if (bothHaveSameValidOwner && bothHaveSameValidKey)
                {
                    return true;
                }
            }
            catch { /* ignore */ }

            return ReferenceEquals(first, second);
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
                    IList<Type> varTypes = variableProperty.VariableTypes;
                    bool shouldCheckForAllTypes = varTypes.Count == 0;
                    if (shouldCheckForAllTypes)
                    {
                        // Include both legacy and muscariable types
                        var all = new List<Type>();
                        all.AddRange(VariableTypeRegistry.AllLegacyTypes);
                        all.AddRange(VariableTypeRegistry.AllMuscariableTypes);
                        typeListToCheck = all;
                    }
                    else
                    {
                        typeListToCheck = variableProperty.VariableTypes;
                    }

                    whetherItDoesOrNot = typeListToCheck.Any((typeInList) => typeInList.IsAssignableFrom(typeToCheck));
                    // ^For polymorphism, we check assignability
                }

                return whetherItDoesOrNot;
            }

            VariableEditor.VariableField(property, 
                                         label,
                                         EditorSelectionTracker.ActiveFlowchart,
                                         variableProperty.defaultText,
                                         ShouldBeAnOptionInTheDropdown,
                                         (lbl, idx, options) => EditorGUI.Popup(position, lbl, idx, options));

            // Commit changes defensively
            property.serializedObject?.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }
    }

    
}
