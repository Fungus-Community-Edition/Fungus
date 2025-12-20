using Amanita.EditorUtils;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

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

            var varDataObj = varDataProp.boxedValue;
            if (varDataObj == null)
            {
                // If the managed reference has not been initialized yet, bail out safely
                EditorGUI.EndProperty();
                return;
            }
            var varData = varDataObj as VariableData;
            if (varData == null)
            {
                // Unexpected type; bail out to avoid downstream NREs
                EditorGUI.EndProperty();
                return;
            }

            // Sub-properties
            var literalValueProp = varDataProp.FindPropertyRelative("value");
            var backingVarRefProp = varDataProp.FindPropertyRelative("backingVarRef");
            if (backingVarRefProp == null)
            {
                // Missing backing reference; cannot proceed safely
                EditorGUI.EndProperty();
                return;
            }
            var itemIdProp = backingVarRefProp.FindPropertyRelative("itemId");

            // Layout
            Rect wholeFieldRect, valueRect, popupRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                wholeFieldRect = EditorGUI.PrefixLabel(position, label);
                valueRect = wholeFieldRect;
                valueRect.width = Mathf.Max(0, wholeFieldRect.width - SpaceForPopup);
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
                bool valChanged = EditorGUI.PropertyField(valueRect, literalValueProp, GUIContent.none);
                if (valChanged)
                {
                    literalValueProp.serializedObject.ApplyModifiedProperties();
                }
            }

            // Flowchart is useful for listing vars, but do not force owner to it
            Flowchart localFlowchart = FlowchartWindow.GetFlowchart();
            string warningMessage;
            if (localFlowchart == null)
            {
                warningMessage = $"No flowchart is open in the Flowchart window. Cannot draw variable " +
                    $"reference field for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                EditorGUI.indentLevel = prevIndent;
                EditorGUI.EndProperty();
                return;
            }

            Type contentType = varData.ContentType;
            if (contentType == null)
            {
                warningMessage = $"Could not resolve ContentType for VariableData drawer " +
                    $"for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                EditorGUI.indentLevel = prevIndent;
                EditorGUI.EndProperty();
                return;
            }

            IVariable selectedVariable = varData.VarRef;

            // Build options
            var ammieManager = AmanitaManager.S;
            var _validVarsOrdered = new Dictionary<string, IVariable>();
            var _labelsSeen = new HashSet<string>();
            RegisterValidVars();
            void RegisterValidVars()
            {
                var validVars = ammieManager.VariableRegistry.GetVarsOfType(contentType);
                _validVarsOrdered.Clear();
                _labelsSeen.Clear();
                AddOption("<Value>", null);

                for (int i = 0; i < validVars.Count; i++)
                {
                    var pair = validVars.ElementAt(i);
                    string label = pair.Key;
                    var variable = pair.Value;

                    if (_labelsSeen.Contains(label))
                    {
                        label = $"{label} (ID:{variable.ItemId})";
                        if (_labelsSeen.Contains(label))
                        {
                            warningMessage = $"Variable label collision could not be resolved for variable {variable.Key} " +
                                $"when adding to dropdown for {varDataProp.propertyPath}. Skipping duplicate.";
                            Debug.LogWarning(warningMessage);
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
                        warningMessage = $"VariableDataDrawer: Variable key collision when adding {label} to " +
                            $"dropdown for {varDataProp.propertyPath}. Skipping duplicate.";
                        Debug.LogWarning(warningMessage);
                        return;
                    }
                    _validVarsOrdered.Add(label, variable);
                }
            }

            bool noVarsFound = _validVarsOrdered.Count == 0;
            if (!shouldDrawLiteral && noVarsFound)
            {
                EditorGUI.indentLevel = prevIndent;
                EditorGUI.EndProperty();
                return;
            }

            // Find selected index
            int selectedIndex = FindSelectedIndex();
            int FindSelectedIndex()
            {
                int idx = 0;
                if (selectedVariable != null)
                {
                    foreach (var kvp in _validVarsOrdered)
                    {
                        var orderedVar = kvp.Value;
                        bool isSelected = false;
                        if (selectedVariable == null && orderedVar == null)
                        {
                            isSelected = true;
                        }
                        else if (selectedVariable != null && orderedVar != null)
                        {
                            bool sameKey = selectedVariable.Key == orderedVar.Key;
                            bool sameContentType = selectedVariable.ContentType.Equals(orderedVar.ContentType);
                            bool sameOwner = ReferenceEquals(selectedVariable.Owner, orderedVar.Owner)
                                || selectedVariable.Owner == null;
                            if (sameKey && sameContentType && sameOwner)
                            {
                                isSelected = true;
                            }
                        }
                        if (isSelected)
                        {
                            return idx;
                        }
                        idx++;
                    }
                }
                return idx;
            }
            
            // Draw popup
            string[] options = _validVarsOrdered.Select(kvp => kvp.Key).ToArray();
            int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
            if (prevSelectedIndex < 0) prevSelectedIndex = 0;
            if (!shouldDrawLiteral) popupRect = wholeFieldRect;
            selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);

            // Apply selection by writing VariableReference owner + id, preserving VSA owners
            var varsOrderedArray = _validVarsOrdered.Values.ToArray();
            IVariable chosenNow = varsOrderedArray[selectedIndex];
            bool choseLiteralValue = chosenNow == null;
            bool choseDiffVar = !choseLiteralValue && itemIdProp.intValue != chosenNow.ItemId;

            if (choseDiffVar)
            {
                Debug.Log($"VariableDataDrawer: Variable selection changed to {chosenNow.Key}" +
                    $"for {varDataProp.propertyPath}.");
            }

            // Update owner fields on backing varRef
            SerializedProperty owningFcProp = backingVarRefProp.FindPropertyRelative("owningFc");
            SerializedProperty owningVsaProp = backingVarRefProp.FindPropertyRelative("owningVsa");

            if (choseLiteralValue)
            {
                // Leave Flowchart owner to current local flowchart to keep context; clear VSA owner
                owningFcProp.objectReferenceValue = localFlowchart;
                owningVsaProp.objectReferenceValue = null;
                itemIdProp.intValue = Variable.InvalidID;
            }
            else
            {
                var vOwner = chosenNow.Owner;
                var fChart = vOwner as Flowchart;
                var vsa = vOwner as VariableSourceAsset;

                owningFcProp.objectReferenceValue = fChart;
                owningVsaProp.objectReferenceValue = vsa;
                itemIdProp.intValue = chosenNow.ItemId;
            }

            // Refresh the runtime view from the backing reference (no owner overwrite)
            varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();
            varDataProp.boxedValue = varData;

            EditorGUI.indentLevel = prevIndent;
            EditorGUI.EndProperty();
        }

        private static readonly int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight); // <- Width of the little button for the popup
        private static readonly int popupGap = 5; // <- Between the value/ref field and the little button for the popup
        private static int SpaceForPopup => popupWidth + popupGap;

    }

    [CustomPropertyDrawer(typeof(AnyVariableData), true)]
    public class AnyVariableDataDrawer : VariableDataDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            var typedUnderlyingDataProp = varDataProp.FindPropertyRelative("data");
            if (typedUnderlyingDataProp == null)
            {
                EditorGUI.BeginProperty(position, label, varDataProp);
                EditorGUI.HelpBox(position, $"Could not find 'data' property for AnyVariableData drawer " +
                    $"for {varDataProp.propertyPath}.", MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            base.OnGUI(position, typedUnderlyingDataProp, label);
        }
    }
}