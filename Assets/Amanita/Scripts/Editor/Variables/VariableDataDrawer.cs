using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer<T> : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);
            VariableData varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();
            varDataProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Find the two key sub-properties
            SerializedProperty valueProp, referenceProp;
            try
            {
                valueProp = varDataProp.FindPropertyRelative("valOfType");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception trying to find 'valOfType' property relative to {varDataProp.propertyPath}. " +
                    $"Its display name: {varDataProp.displayName}. Make sure the VariableData class still has a field named 'valOfType'. Exception: {e}");
                throw;
            }
            referenceProp = varDataProp.FindPropertyRelative("varRef");

            // Layout: label, then value/reference side-by-side
            int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
            const int popupGap = 5;
            Rect wholeFieldRect = EditorGUI.PrefixLabel(position, label);
            Rect valueRect = wholeFieldRect;
            int spaceForPopup = popupWidth + popupGap;
            valueRect.width = Mathf.Max(0, wholeFieldRect.width - spaceForPopup);
            // ^We want to make sure that the rect for the value field leaves enough space for the popup
            Rect popupRect = wholeFieldRect; 
            popupRect.x += valueRect.width + popupGap;
            popupRect.width = popupWidth;

            int prevIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // We only want to draw the literal value when the varRef is null
            bool shouldDrawLiteral = !VarRefPropHasAnythingAssigned(referenceProp);
            if (shouldDrawLiteral)
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);

            // Draw the variable reference (branch on propertyType)
            // Going to need to define some new logic here, since the old stuff was predicated
            // on the var refs having VariableInfo attributes, which they no longer do.

            DrawReferenceField();
            void DrawReferenceField()
            {
                Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
                if (localFlowchart == null)
                {
                    Debug.LogWarning($"No flowchart is open in the Flowchart window. Cannot draw variable reference field for {varDataProp.propertyPath}.");
                    return;
                }

                int index = 0, selectedIndex = 0;
                // ^So we can track which var in the dropdown is currently selected
                IVariablePointer selectedVariable = referenceProp.boxedValue as IVariablePointer;

                RegisterValidVars();
                void RegisterValidVars()
                {
                    var dataAttr = varData.GetType().GetCustomAttribute<VariableDataAttribute>();
                    if (dataAttr == null)
                    {
                        Debug.LogWarning($"VariableDataAttribute for {varData.GetType().Name} not found. May be sign of underlying problem.");
                        return;
                    }
                    var contentType = dataAttr.ContentType;
                    validVarLookup.Clear();
                    validVarLookup.Add("<Value>", null); // Option to switch back to literal value

                    RegisterLocalVars();
                    void RegisterLocalVars()
                    {
                        IList<IVariable> validLocalVars = localFlowchart.Variables
                            .Where(elem => elem.ContentType.Equals(contentType))
                            .ToList();
                        for (int i = 0; i < validLocalVars.Count; i++)
                        {
                            var elem = validLocalVars[i];
                            if (!validVarLookup.ContainsKey(elem.Key))
                            {
                                validVarLookup.Add(elem.Key, elem);
                            }
                            else
                            {
                                Debug.LogWarning($"Variable key collision when trying to add variable {elem.Key} to the dropdown for {varDataProp.propertyPath}. " +
                                    $"There is already a variable with that key in the dropdown. Skipping this one.");
                            }

                            index++;

                            // Since selectedVariable a VariablePointer, we'd best go with semantic equality.
                            // And make sure to call it from selectedVariable, given how VariablePointer.Equals is implemented.
                            if (selectedVariable != null && selectedVariable.Equals(elem)) 
                            {
                                selectedIndex = index;
                                Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                            }
                        }
                    }

                    RegisterVarsFromOtherFlowcharts();
                    void RegisterVarsFromOtherFlowcharts()
                    {
                        IList<Flowchart> otherFlowchartsInScene = Flowchart.CachedFlowcharts.Where
                            ((elem) => elem != localFlowchart).ToList();
                        for (int i = 0; i < otherFlowchartsInScene.Count; i++)
                        {
                            var otherChart = otherFlowchartsInScene[i];
                            IList<IVariable> validVarsInOtherChart = otherChart.Variables
                                .Where(elem => elem.ContentType.Equals(contentType) && elem.Scope == VariableScope.Public)
                                .ToList();
                            for (int j = 0; j < validVarsInOtherChart.Count; j++)
                            {
                                var elem = validVarsInOtherChart[j];
                                string namespacedKey = $"{otherChart.gameObject.name}/{elem.Key}";
                                if (!validVarLookup.ContainsKey(namespacedKey))
                                {
                                    validVarLookup.Add(namespacedKey, elem);
                                }
                                else
                                {
                                    Debug.LogWarning($"Variable key collision when trying to add variable {namespacedKey} to the dropdown for {varDataProp.propertyPath}. " +
                                        $"There is already a variable with that key in the dropdown. Skipping this one.");
                                }

                                index++;

                                if (selectedVariable != null && selectedVariable.Equals(elem))
                                {
                                    selectedIndex = index;
                                    Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                                }
                            }
                        }
                    }

                    RegisterGlobalVars();
                    void RegisterGlobalVars()
                    {
                        GlobalVariables globalVars = AmanitaManager.S.GlobalVariables;
                        IList<IVariable> validGlobalVars = globalVars.Variables
                            .Where(elem => elem.ContentType.Equals(contentType))
                            .ToList();

                        for (int i = 0; i < validGlobalVars.Count; i++)
                        {
                            var elem = validGlobalVars[i];
                            string namespacedKey = $"Global/{elem.Key}";
                            if (!validVarLookup.ContainsKey(namespacedKey))
                            {
                                validVarLookup.Add(namespacedKey, elem);
                            }
                            else
                            {
                                Debug.LogWarning($"Variable key collision when trying to add variable {namespacedKey} to the dropdown for {varDataProp.propertyPath}. " +
                                    $"There is already a variable with that key in the dropdown. Skipping this one.");
                            }

                            index++;

                            if (selectedVariable != null && selectedVariable.Equals(elem))
                            {
                                selectedIndex = index;
                                Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                            }
                        }
                    }
                }

                bool noVarsFound = validVarLookup.Count == 0;
                if (noVarsFound)
                {
                    return;
                }
                IList<string> options = validVarLookup.Keys.ToList();
                int prevSelectedIndex = Mathf.Min(options.Count, selectedIndex);
                
                IVariable chosenBefore = validVarLookup[options[prevSelectedIndex]];

                if (!shouldDrawLiteral && chosenBefore != null)
                {
                    popupRect = wholeFieldRect;
                }

                selectedIndex = EditorGUI.Popup(popupRect, selectedIndex, options.ToArray());

                IVariable chosenNow = validVarLookup[options[selectedIndex]];
                referenceProp.AssignVarRef(chosenNow, varData.ContentType);

                if (selectedIndex != prevSelectedIndex)
                {
                    Debug.Log($"Selected something else");
                }

                if (chosenNow != null)
                {

                }

            }

            EditorGUI.indentLevel = prevIndent;

            EditorGUI.EndProperty();
        }

        protected IDictionary<string, IVariable> validVarLookup = new Dictionary<string, IVariable>();

        protected virtual bool VarRefPropHasAnythingAssigned(SerializedProperty varRefProp)
        {
            bool result = false;

            switch (varRefProp.propertyType)
            {
                case SerializedPropertyType.ObjectReference:
                    // UnityEngine.Object or ScriptableObject-backed variable
                    result = varRefProp.objectReferenceValue != null;
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
                    // [SerializeReference] polymorphic variable
                    result = varRefProp.managedReferenceValue != null;
                    break;

                default:
                    Debug.LogError($"[VarRefPropHasAnythingAssigned] Did not account for var ref prop being of serialized property type {varRefProp.propertyType}");
                    break;
            }

            return result;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var referenceProp = property.FindPropertyRelative("varRef");
            if (referenceProp != null && referenceProp.propertyType == SerializedPropertyType.ManagedReference)
            {
                // Let Unity calculate height for polymorphic managed refs
                return EditorGUI.GetPropertyHeight(referenceProp, true);
            }
            return EditorGUIUtility.singleLineHeight;
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