using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using Type = System.Type;

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

            // Find the two key sub-properties
            SerializedProperty literalValueProp, referenceVarProp;
            string litValuePropName = "value", refPropName = "varRef";
            literalValueProp = varDataProp.FindPropertyRelative(litValuePropName);
            referenceVarProp = varDataProp.FindPropertyRelative(refPropName);

            // Layout: label, then value/reference side-by-side
            Rect valueRect, popupRect, wholeFieldRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
                const int popupGap = 5; // <- Between the value/ref field and the little button for the popup
                wholeFieldRect = EditorGUI.PrefixLabel(position, label);
                valueRect = wholeFieldRect;
                int spaceForPopup = popupWidth + popupGap;
                valueRect.width = Mathf.Max(0, wholeFieldRect.width - spaceForPopup);
                // ^We want to make sure that the rect for the value field leaves enough space for the popup
                popupRect = wholeFieldRect;
                popupRect.x += valueRect.width + popupGap;
                popupRect.width = popupWidth;

                prevIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
            }

            // We only want to draw the literal value when the varRef is null
            bool shouldDrawLiteral = !VarRefPropHasAnythingAssigned(referenceVarProp);
            if (shouldDrawLiteral)
            {
                EditorGUI.PropertyField(valueRect, literalValueProp, GUIContent.none);
            }

            Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
            if (localFlowchart == null)
            {
                Debug.LogWarning($"No flowchart is open in the Flowchart window. Cannot draw variable reference field for {varDataProp.propertyPath}.");
                return;
            }

            #region Resolve Content Type
            Type contentType = GetContentType();
            Type GetContentType()
            {
                var dataAttr = varData.GetType().GetCustomAttribute<VariableDataAttribute>();
                Type result = dataAttr != null ? dataAttr.ContentType : varData.ContentType;
                return result;
            }

            if (contentType == null)
            {
                Debug.LogWarning($"Unable to resolve ContentType for {varData.GetType().Name}. Showing only literal <Value> option.");
                return;
            }
            #endregion

            int selectedIndex = 0;
            IVariable selectedVariable = referenceVarProp.boxedValue as IVariable;

            // Regardless of whether we are drawing the literal value or not, we need to populate the list of valid vars
            // so we know what to show in the popup.
            RegisterValidVars(); // Valid to be assigned to the VariableData we are drawing for, to be specific
            void RegisterValidVars()
            {
                _validVarsOrdered.Clear();
                _labelsSeen.Clear();
                AddOption("<Value>", null); // To let the user go with a literal val instead of a var

                RegisterLocalVars();
                void RegisterLocalVars()
                {
                    IList<IVariable> validLocalVars = localFlowchart.Variables
                        .Where(elem => contentType.IsAssignableFrom(elem.ContentType))
                        .ToList();

                    for (int i = 0; i < validLocalVars.Count; i++)
                    {
                        var elem = validLocalVars[i];
                        AddOption(elem.Key, elem);
                    }
                }

                RegisterPublicVarsFromOtherFlowcharts();
                void RegisterPublicVarsFromOtherFlowcharts()
                {
                    IList<Flowchart> otherFlowchartsInScene = Flowchart.CachedFlowcharts.Where
                        ((elem) => elem != localFlowchart).ToList();

                    for (int i = 0; i < otherFlowchartsInScene.Count; i++)
                    {
                        var otherChart = otherFlowchartsInScene[i];
                        IList<IVariable> validVarsInOtherChart = otherChart.Variables
                            .Where(elem => elem.ContentType.IsAssignableFrom(contentType)
                            && elem.Scope == VariableScope.Public)
                            .ToList();

                        for (int j = 0; j < validVarsInOtherChart.Count; j++)
                        {
                            var elem = validVarsInOtherChart[j];
                            string namespacedKey = $"{otherChart.gameObject.name}/{elem.Key}";
                            // ^So we can tell which vars belong to which Flowcharts
                            AddOption(namespacedKey, elem);
                        }
                    }
                }

                RegisterGlobalVars();
                void RegisterGlobalVars()
                {
                    var ammieManager = AmanitaManager.S;
                    if (ammieManager == null)
                    {
                        return;
                    }

                    var varSources = ammieManager.GlobalVariableSources;
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
                            // ^This makes it easy for the user to organize their global vars by source asset instead
                            // of having to sift through one long list. And of course, the tilde (~) indicates global scope.
                            AddOption(namespacedKey, elem);
                        }
                    }
                }

                void AddOption(string label, IVariable variable)
                {
                    if (_validVarsOrdered.ContainsKey(label))
                    {
                        Debug.LogWarning($"Variable key collision when trying to add variable {label} to " +
                            $"the dropdown for {varDataProp.propertyPath}. Skipping duplicate.");
                        return;
                    }

                    _validVarsOrdered.Add(label, variable);
                }
            }

            bool noVarsFound = _validVarsOrdered.Count == 0;
            if (!shouldDrawLiteral && noVarsFound)
            {
                return;
            }

            // Find the index of the currently selected variable (if any)
            FindSelectedVariableIndex();
            void FindSelectedVariableIndex()
            {
                if (selectedVariable != null)
                {
                    int foundIndex = 0;
                    foreach (var kvp in _validVarsOrdered)
                    {
                        if (kvp.Value == null)
                        {
                            foundIndex++;
                            continue;
                        }

                        // It's possible that the VariableData is referencing a variable that's a copy of the one
                        // on the Flowchart (e.g. if the Flowchart was duplicated). Thus, we compare by certain fields.
                        var orderedVar = kvp.Value;
                        bool sameKey = selectedVariable.Key == orderedVar.Key;
                        bool sameContentType = selectedVariable.ContentType.Equals(orderedVar.ContentType);
                        bool sameOwner = ReferenceEquals(selectedVariable.Owner, orderedVar.Owner) ||
                            selectedVariable.Owner == null; // It's possible that the copy's owner was nulled, and thus...
                        bool isSameVar = orderedVar != null && sameKey && sameContentType && sameOwner;
                        if (isSameVar)
                        {
                            selectedIndex = foundIndex;
                            selectedVariable = kvp.Value; // To keep the exact instance from the Flowchart.
                            break;
                        }
                        foundIndex++;
                    }
                }
            }

            DrawReferenceField();
            void DrawReferenceField()
            {
                string[] options = _validVarsOrdered.Select(kvp => kvp.Key).ToArray();

                int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
                if (prevSelectedIndex < 0)
                {
                    prevSelectedIndex = 0;
                }

                if (referenceVarProp.boxedValue is IVariable existing && existing != null)
                {
                    // Keep popup full-width if a reference is already chosen
                    popupRect = wholeFieldRect;
                }

                selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);

                var varsOrderedArray = _validVarsOrdered.Values.ToArray();
                IVariable chosenNow = varsOrderedArray[selectedIndex];

                // IMPORTANT: call the overload that triggers VariableData.VarRef setter
                referenceVarProp.AssignVarRef(varData, chosenNow, varData.ContentType);
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
                    result = varRefProp.objectReferenceValue != null;
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.ManagedReference:
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
                return EditorGUI.GetPropertyHeight(referenceProp, true);
            }
            return EditorGUIUtility.singleLineHeight;
        }

        protected readonly Dictionary<string, IVariable> _validVarsOrdered = new Dictionary<string, IVariable>();
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