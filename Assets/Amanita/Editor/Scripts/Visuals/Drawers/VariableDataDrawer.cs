using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer : PropertyDrawer
    {
        // Note that each subclass of PropertyDrawer is treated as a singleton of sorts by Unity's
        // internals. Thus, best avoid giving these instance members that can hold state between calls.
        // Unless that state is immutable or reset at the start of each OnGUI call.

        protected readonly DefaultEditorAssetResolver _assetResolver = new DefaultEditorAssetResolver();

        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);
            VariableData varData = varDataProp.boxedValue as VariableData;
            // ^If we play our cards right, we can indeed use this to modify the
            // actual instance inside the serialized property. Only the non-serialized
            // properties should get reset on reloads or otherwise after this frame.

            SerializedProperty literalValueProp, itemIdProp;
            string litValuePropName = "value", itemIdPropName = "storedItemId";
            literalValueProp = varDataProp.FindPropertyRelative(litValuePropName);
            itemIdProp = varDataProp.FindPropertyRelative(itemIdPropName);

            Rect wholeFieldRect, valueRect, popupRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                // Label, then value/reference side-by-side
                int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
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
            // If the var datas is meant to represent a var, its stored item id should be a valid one
            bool validStoredItemId = itemIdProp != null && itemIdProp.intValue != Variable.InvalidID;
            bool shouldDrawLiteral = !validStoredItemId;
            if (shouldDrawLiteral)
            {
                DrawLiteralValueProp(literalValueProp);
            }

            void DrawLiteralValueProp(SerializedProperty literalValueProp)
            {
                bool valChanged = EditorGUI.PropertyField(valueRect, literalValueProp, GUIContent.none);

                if (valChanged)
                {
                    Debug.Log("Value changed in literal field.");
                }

                literalValueProp.serializedObject.ApplyModifiedProperties();
            }

            Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
            if (localFlowchart == null)
            {
                string warningMessage = $"No flowchart is open in the Flowchart window. Cannot draw " +
                    $"variable reference field for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
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
                string warningMessage = $"Could not resolve ContentType for VariableData drawer for " +
                    $"{varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                return;
            }
            #endregion

            int selectedIndex = 0;
            varData.VarOwner = localFlowchart; // To make sure we can get the right variable
            IVariable selectedVariable = varData.VarRef;

            // Regardless of whether we are drawing the literal value or not, we need to populate the list of valid vars
            // so we know what to show in the popup.
            Dictionary<string, IVariable> _validVarsOrdered = new Dictionary<string, IVariable>();
            HashSet<string> _labelsSeen = new HashSet<string>();
            var ammieManager = AmanitaManager.S;
            RegisterValidVars(); // Valid to be assigned to the VariableData we are drawing for, to be specific
            void RegisterValidVars()
            {
                var varRegistry = ammieManager.VariableRegistry;
                IReadOnlyDictionary<string, IVariable> validVars = varRegistry.GetVarsOfType(contentType);
                // ^Note that the keys here mention the owners when appropriate, and thus we don't 
                // have to set those up ourselves
                _validVarsOrdered.Clear();
                _labelsSeen.Clear();
                AddOption("<Value>", null); // To let the user go with a literal val instead of a var

                // Add the options one by one
                for (int i = 0; i < validVars.Count; i++)
                {
                    var pair = validVars.ElementAt(i);
                    string label = pair.Key;
                    IVariable variable = pair.Value;
                    string varKey = variable.Key;

                    // Ensure uniqueness of labels
                    if (_labelsSeen.Contains(label))
                    {
                        // Try to disambiguate by adding the variable's ItemId
                        label = $"{label} (ID:{variable.ItemId})";
                        if (_labelsSeen.Contains(label))
                        {
                            Debug.LogWarning($"Variable label collision for variable {varKey} from owner  " +
                                $"when trying to add to the dropdown for {varDataProp.propertyPath}. Skipping duplicate.");
                            continue;
                        }
                    }
                    _labelsSeen.Add(label);
                    AddOption(label, variable);
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

            // Find the index of the currently selected variable (if any).
            // This will help us make sure that the popup shows the correct selection.
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

            DrawPopupField();
            void DrawPopupField()
            {
                string[] options = _validVarsOrdered.Select(kvp => kvp.Key).ToArray();

                int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
                if (prevSelectedIndex < 0)
                {
                    prevSelectedIndex = 0;
                }

                if (!shouldDrawLiteral)
                {
                    popupRect = wholeFieldRect;
                    // ^In this case, we need to make the popup take up the full width so we can see
                    // the selected var's label properly.
                }

                selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);
            }

            UpdateItemIdPropBasedOnSelection();
            void UpdateItemIdPropBasedOnSelection()
            {
                var varsOrderedArray = _validVarsOrdered.Values.ToArray();
                IVariable chosenNow = varsOrderedArray[selectedIndex];
                bool choseLiteralValue = chosenNow == null;
                if (choseLiteralValue)
                {
                    itemIdProp.intValue = Variable.InvalidID;
                }
                else
                {
                    itemIdProp.intValue = chosenNow.ItemId;
                }

                varData = varDataProp.boxedValue as VariableData;
                // ^It's possible that the literal value changed before this point. Thus, to make sure we're working
                // with the most accurate var data, we refetch it here.
                varData.VarRef = chosenNow;
                varDataProp.boxedValue = varData; 
                // ^Despite how we got varData from varDataProp.boxedValue, 
                // we need to set it back to ensure changes are registered.
                varDataProp.serializedObject.ApplyModifiedProperties();
                
                EditorUtility.SetDirty(varDataProp.serializedObject.targetObject);
            }

            EditorGUI.indentLevel = prevIndent;
            EditorGUI.EndProperty();
        }

        private static readonly int popupGap = 5; // <- Between the value/ref field and the little button for the popup

    }

    [CustomPropertyDrawer(typeof(AnyVariableData), true)]
    public class AnyVariableDataDrawer : VariableDataDrawer
    {
    }
}