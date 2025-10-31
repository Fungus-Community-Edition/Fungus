using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer<T> : PropertyDrawer
    {
        protected readonly DefaultEditorAssetResolver _assetResolver = new DefaultEditorAssetResolver();

        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);
            VariableData varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();
            varDataProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Find the two key sub-properties
            SerializedProperty valueProp, referenceProp;
            string valuePropName = "value";
            try
            {
                valueProp = varDataProp.FindPropertyRelative(valuePropName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception trying to find 'value' property relative to {varDataProp.propertyPath}. " +
                    $"Its display name: {varDataProp.displayName}. Make sure the VariableData class still has a field named '{valuePropName}'. Exception: {e}");
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
            {
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none);
            }

            DrawReferenceField();
            void DrawReferenceField()
            {
                Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
                if (localFlowchart == null)
                {
                    Debug.LogWarning($"No flowchart is open in the Flowchart window. Cannot draw variable reference field for {varDataProp.propertyPath}.");
                    return;
                }

                int selectedIndex = 0;
                IVariable selectedVariable = referenceProp.boxedValue as IVariable;

                RegisterValidVars();
                void RegisterValidVars()
                {
                    var dataAttr = varData.GetType().GetCustomAttribute<VariableDataAttribute>();
                    if (dataAttr == null)
                    {
                        Debug.LogWarning($"VariableDataAttribute for {varData.GetType().Name} not found. " +
                            $"May be sign of underlying problem.");
                        return;
                    }

                    var contentType = dataAttr.ContentType;

                    _validVarsOrdered.Clear();
                    _labelsSeen.Clear();

                    // Always include the <Value> option first so the user can select it to enter a literal
                    AddOption("<Value>", null);

                    RegisterLocalVars();
                    void RegisterLocalVars()
                    {
                        IList<IVariable> validLocalVars = localFlowchart.Variables
                            .Where(elem => contentType.IsAssignableFrom(elem.ContentType)) // Polymorphism allowed
                            .ToList();

                        for (int i = 0; i < validLocalVars.Count; i++)
                        {
                            var elem = validLocalVars[i];
                            AddOption(elem.Key, elem);

                            int idx = _validVarsOrdered.Count - 1;
                            if (selectedVariable != null && ReferenceEquals(selectedVariable, elem))
                            {
                                selectedIndex = idx;
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
                                .Where(elem => elem.ContentType.Equals(contentType) 
                                && elem.Scope == VariableScope.Public)
                                .ToList();

                            for (int j = 0; j < validVarsInOtherChart.Count; j++)
                            {
                                var elem = validVarsInOtherChart[j];
                                string namespacedKey = $"{otherChart.gameObject.name}/{elem.Key}";
                                // ^So we know which vars belong to which Flowcharts
                                AddOption(namespacedKey, elem);

                                int idx = _validVarsOrdered.Count - 1;
                                if (selectedVariable != null && ReferenceEquals(selectedVariable, elem))
                                {
                                    selectedIndex = idx;
                                    Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                                }
                            }
                        }
                    }

                    RegisterGlobalVars();
                    void RegisterGlobalVars()
                    {
                        var ammieManager = AmanitaManager.S;
                        var varSources = ammieManager.GlobalVariableSources;
                        // ^So we can specify which vars come from which sources
                        for (int i = 0; i < varSources.Count; i++)
                        {
                            var source = varSources[i];
                            IList<IVariable> validVarsInSource = source.Variables
                                .Where(elem => contentType.IsAssignableFrom(elem.ContentType))
                                .ToList();
                            for (int j = 0; j < validVarsInSource.Count; j++)
                            {
                                var elem = validVarsInSource[j];
                                string namespacedKey = $"~{source.name}~/{elem.Key}";
                                // ^Rather than make a group under Globals, we just enclose the source's name in tildes
                                // to indicate it's globalness. In the docs, we might want to recommend that users
                                // avoid tildes in their Flowchart names to prevent confusion.
                                AddOption(namespacedKey, elem);
                                int idx = _validVarsOrdered.Count - 1;
                                if (selectedVariable != null && ReferenceEquals(selectedVariable,  elem))
                                {
                                    selectedIndex = idx;
                                    Debug.Log($"Found selected variable {elem.Key} at index {selectedIndex} in dropdown for {varDataProp.propertyPath}");
                                }
                            }
                        }
                    }

                    void AddOption(string label, IVariable variable)
                    {
                        if (_labelsSeen.Contains(label))
                        {
                            Debug.LogWarning($"Variable key collision when trying to add variable {label} to the " +
                                $"dropdown for {varDataProp.propertyPath}. There is already a variable with that " +
                                $"key in the dropdown. Skipping this one.");
                            return;
                        }

                        _labelsSeen.Add(label);
                        _validVarsOrdered.Add(new KeyValuePair<string, IVariable>(label, variable));
                    }
                }

                bool noVarsFound = _validVarsOrdered.Count == 0;
                if (!shouldDrawLiteral && noVarsFound)
                {
                    return;
                }

                string[] options = _validVarsOrdered.Select(kvp => kvp.Key).ToArray();

                // Clamp to valid range to avoid out-of-range when nothing was matched
                int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
                IVariable chosenBefore = _validVarsOrdered[prevSelectedIndex].Value;

                if (chosenBefore != null)
                {
                    popupRect = wholeFieldRect;
                }

                selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);

                if (selectedIndex != prevSelectedIndex)
                {
                    Debug.Log($"Selected something else");
                }

                IVariable chosenNow = _validVarsOrdered[selectedIndex].Value;
                referenceProp.AssignVarRef(chosenNow, varData.ContentType);
            }
            
            
            EditorGUI.indentLevel = prevIndent;

            EditorGUI.EndProperty();
        }

        protected UnityObj _variableSourceContext;

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

        // Keep ordered options separate from de-dup tracking
        protected readonly List<KeyValuePair<string, IVariable>> _validVarsOrdered = new List<KeyValuePair<string, IVariable>>();
        protected readonly HashSet<string> _labelsSeen = new HashSet<string>();
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