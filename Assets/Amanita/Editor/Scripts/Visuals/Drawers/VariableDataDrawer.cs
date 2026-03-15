using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Type = System.Type;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer : PropertyDrawer
    {
        private const bool LogDrawer = true;

        // Note that each subclass of PropertyDrawer is treated as a singleton of sorts by Unity's
        // internals. Thus, best avoid giving these instance members that can hold state between calls.
        // Unless that state is immutable or reset at the start of each OnGUI call.

        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);

            float prevLabelWidth = EditorGUIUtility.labelWidth;

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
            Rect labelRect, valueRect, popupRect, fieldRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                labelRect = new Rect(position.x, position.y, labelWidth, position.height);

                fieldRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);
                if (fieldRect.width < MinimumValueWidth + SpaceForPopup)
                {
                    fieldRect = new Rect(position.x, position.y, position.width, position.height);
                    labelRect.width = 0f;
                }

                valueRect = fieldRect;
                valueRect.width = Mathf.Max(0, fieldRect.width - SpaceForPopup);
                popupRect = new Rect(fieldRect.x + valueRect.width + popupGap, fieldRect.y, popupWidth, fieldRect.height);

                prevIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
            }

            void RestoreLayout()
            {
                EditorGUI.indentLevel = prevIndent;
                EditorGUIUtility.labelWidth = prevLabelWidth;
            }

            // We only want to draw the literal value when the varRef is null
            // If the var datas is meant to represent a var, its stored item id should be a valid one
            bool validStoredItemId = itemIdProp != null && itemIdProp.intValue != Variable.InvalidID;
            bool shouldDrawLiteral = !validStoredItemId;

            if (LogDrawer)
            {
                //Debug.Log($"VariableDataDrawer[{varDataProp.propertyPath}] pos={position} labelWidth={EditorGUIUtility.labelWidth} " +
                //          $"valueRect={valueRect} popupRect={popupRect} itemId={itemIdProp?.intValue} " +
                //          $"shouldDrawLiteral={shouldDrawLiteral} literalPropType={literalValueProp?.propertyType}");
            }

            if (labelRect.width > 0f)
            {
                EditorGUI.LabelField(labelRect, label);
            }

            if (shouldDrawLiteral)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(valueRect, literalValueProp, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    literalValueProp.serializedObject.ApplyModifiedProperties();
                }
            }

            // Flowchart is useful for listing vars, but do not force owner to it
            Flowchart localFlowchart = EditorSelectionTracker.ActiveFlowchart;
            if (localFlowchart == null)
            {
                GameObject selectedGo = Selection.activeGameObject;
                if (selectedGo != null)
                {
                    localFlowchart = selectedGo.GetComponent<Flowchart>();
                }
            }
            string warningMessage;
            if (localFlowchart == null)
            {
                warningMessage = $"No flowchart is open in the Flowchart window. Cannot draw variable " +
                    $"reference field for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                RestoreLayout();
                EditorGUI.EndProperty();
                return;
            }

            Type contentType = varData.ContentType;
            if (contentType == null)
            {
                warningMessage = $"Could not resolve ContentType for VariableData drawer " +
                    $"for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                RestoreLayout();
                EditorGUI.EndProperty();
                return;
            }

            IVariable selectedVariable = varData.VarRef;

            // Build options
            var ammieManager = AmanitaManager.S;
            var _labelsSeen = new HashSet<string>();
            var orderedLabels = new List<string>();
            var orderedVars = new List<IVariable>();

            RegisterValidVars();
            void RegisterValidVars()
            {
                var validVars = VarRegistry.GetVarsOfType(contentType);
                _labelsSeen.Clear();
                orderedLabels.Clear();
                orderedVars.Clear();

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
                    orderedLabels.Add(label);
                    orderedVars.Add(variable);
                }
            }

            bool noVarsFound = orderedVars.Count == 0;
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
                    for (int i = 0; i < orderedVars.Count; i++)
                    {
                        var orderedVar = orderedVars[i];
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
            string[] options = orderedLabels.ToArray();
            int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
            if (prevSelectedIndex < 0) prevSelectedIndex = 0;
            if (!shouldDrawLiteral) popupRect = fieldRect;

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);
            bool popupChanged = EditorGUI.EndChangeCheck();

            if (LogDrawer)
            {
                //Debug.Log($"VariableDataDrawer[{varDataProp.propertyPath}] popupSelectedIndex={selectedIndex} " +
                //          $"popupLabel={options[selectedIndex]} prevIndex={prevSelectedIndex} " +
                //          $"itemIdBeforeApply={itemIdProp.intValue}");
            }

            // Apply selection only when changed
            if (popupChanged)
            {
                IVariable chosenNow = orderedVars[selectedIndex];
                bool choseLiteralValue = chosenNow == null;

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

                if (LogDrawer)
                {
                    Debug.Log($"VariableDataDrawer[{varDataProp.propertyPath}] apply choseLiteral={choseLiteralValue} " +
                              $"itemIdAfterApply={itemIdProp.intValue} ownerFc={owningFcProp.objectReferenceValue} " +
                              $"ownerVsa={owningVsaProp.objectReferenceValue}");
                }
            }

            // Refresh the runtime view from the backing reference (no owner overwrite)
            varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();

            RestoreLayout();
            EditorGUI.EndProperty();

            varDataProp.serializedObject.ApplyModifiedProperties();
        }

        private static readonly int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight); // <- Width of the little button for the popup
        private static readonly int popupGap = 5; // <- Between the value/ref field and the little button for the popup
        private static int SpaceForPopup => popupWidth + popupGap;
        private static readonly float MinimumValueWidth = 80f;
        private static VariableRegistry VarRegistry => VariableRegistryService.Registry;

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