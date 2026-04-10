using System.Collections.Generic;
using System.Linq;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomPropertyDrawer(typeof(StringData), true)]
    public class StringDataDrawer : VariableDataDrawerBase
    {
        protected override bool UseMultilineLabel(SerializedProperty varDataProp, VariableData varData, bool shouldDrawLiteral)
        {
            if (!shouldDrawLiteral)
            {
                return false;
            }

            if (!ShouldUseTextArea(varData))
            {
                return false;
            }

            HyphlowTextAreaAttribute textAreaAttribute = GetTextAreaAttribute();
            return textAreaAttribute != null && textAreaAttribute.MinLines >= 2;
        }

        private const bool LogDrawer = true;

        public override float GetPropertyHeight(SerializedProperty varDataProp, GUIContent label)
        {
            float baseHeight = EditorGUIUtility.singleLineHeight;
            if (!ShouldDrawLiteral(varDataProp))
            {
                return baseHeight;
            }

            var varData = varDataProp.boxedValue as VariableData;
            if (!ShouldUseTextArea(varData))
            {
                return baseHeight;
            }

            HyphlowTextAreaAttribute textAreaAttribute = GetTextAreaAttribute();
            if (textAreaAttribute == null)
            {
                return baseHeight;
            }

            int lineCount = Mathf.Max(1, textAreaAttribute.MinLines);
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float textAreaHeight = (lineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));

            if (textAreaAttribute.MinLines >= 2)
            {
                return baseHeight + EditorGUIUtility.standardVerticalSpacing + textAreaHeight;
            }

            return textAreaHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty varDataProp, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, varDataProp);

            float prevLabelWidth = EditorGUIUtility.labelWidth;

            var varDataObj = varDataProp.boxedValue;
            if (varDataObj == null)
            {
                EditorGUI.EndProperty();
                return;
            }
            var varData = varDataObj as VariableData;
            if (varData == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            var literalValueProp = varDataProp.FindPropertyRelative("value");
            var backingVarRefProp = varDataProp.FindPropertyRelative("backingVarRef");
            if (backingVarRefProp == null)
            {
                EditorGUI.EndProperty();
                return;
            }
            var itemIdProp = backingVarRefProp.FindPropertyRelative("itemId");

            Rect labelRect, valueRect, popupRect, fieldRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                float labelOffset = (EditorGUI.indentLevel * 15f);

                bool shouldDrawLiteral = itemIdProp == null || itemIdProp.intValue == Variable.InvalidID;
                HyphlowTextAreaAttribute textAreaAttribute = GetTextAreaAttribute();
                bool useMultilineLabel = shouldDrawLiteral &&
                    ShouldUseTextArea(varData) &&
                    textAreaAttribute != null &&
                    textAreaAttribute.MinLines >= 2;

                if (useMultilineLabel)
                {
                    float lineHeight = EditorGUIUtility.singleLineHeight;
                    labelRect = new Rect(position.x + labelOffset, position.y, position.width - labelOffset, lineHeight);

                    float fieldY = position.y + lineHeight + EditorGUIUtility.standardVerticalSpacing;
                    float fieldHeight = position.height - lineHeight - EditorGUIUtility.standardVerticalSpacing;
                    fieldRect = new Rect(position.x + labelOffset, fieldY, position.width - labelOffset, fieldHeight);
                }
                else
                {
                    float labelX = position.x + labelOffset;
                    labelRect = new Rect(labelX, position.y, labelWidth, position.height);

                    float fieldX = position.x + labelWidth + 2;
                    float fieldWidth = position.width - labelWidth;
                    fieldRect = new Rect(fieldX, position.y, fieldWidth, position.height);
                    if (fieldRect.width < MinimumValueWidth + SpaceForPopup)
                    {
                        fieldRect = new Rect(position.x, position.y, position.width, position.height);
                        labelRect.width = 0f;
                    }
                }

                valueRect = fieldRect;
                valueRect.width = Mathf.Max(0, fieldRect.width - SpaceForPopup);
                float popupX = position.x + (position.width - popupWidth);
                popupRect = new Rect(popupX, fieldRect.y, popupWidth, fieldRect.height);

                prevIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
            }

            void RestoreLayout()
            {
                EditorGUI.indentLevel = prevIndent;
                EditorGUIUtility.labelWidth = prevLabelWidth;
            }

            bool validStoredItemId = itemIdProp != null && itemIdProp.intValue != Variable.InvalidID;
            bool shouldDrawLiteral = !validStoredItemId;

            if (LogDrawer)
            {
                //Debug.Log($"StringDataDrawer[{varDataProp.propertyPath}] pos={position} labelWidth={EditorGUIUtility.labelWidth} " +
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
                if (ShouldUseTextArea(varData))
                {
                    GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea)
                    {
                        wordWrap = ShouldWordWrapTextArea()
                    };

                    string newValue = EditorGUI.TextArea(valueRect, literalValueProp.stringValue, textAreaStyle);
                    if (EditorGUI.EndChangeCheck())
                    {
                        literalValueProp.stringValue = newValue;
                        literalValueProp.serializedObject.ApplyModifiedProperties();
                    }
                }
                else
                {
                    EditorGUI.PropertyField(valueRect, literalValueProp, GUIContent.none);
                    if (EditorGUI.EndChangeCheck())
                    {
                        literalValueProp.serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            Flowchart localFlowchart = null;
            FindLocalFlowchart();
            void FindLocalFlowchart()
            {
                if (backingVarRefProp != null)
                {
                    SerializedProperty owningFcProp = backingVarRefProp.FindPropertyRelative("owningSource");
                    if (owningFcProp != null && owningFcProp.objectReferenceValue != null)
                    {
                        var fc = owningFcProp.objectReferenceValue as Flowchart;
                        if (fc != null)
                        {
                            localFlowchart = fc;
                        }
                    }
                }

                if (localFlowchart == null)
                {
                    localFlowchart = EditorSelectionTracker.ActiveFlowchart;
                }

                if (localFlowchart == null)
                {
                    GameObject selectedGo = Selection.activeGameObject;
                    if (selectedGo != null)
                    {
                        localFlowchart = selectedGo.GetComponent<Flowchart>();
                    }
                }

                if (localFlowchart == null && FlowchartWindow.S != null)
                {
                    localFlowchart = FlowchartWindow.S.Flowchart;
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
                warningMessage = $"Could not resolve ContentType for StringData drawer " +
                    $"for {varDataProp.propertyPath}.";
                Debug.LogWarning(warningMessage);
                RestoreLayout();
                EditorGUI.EndProperty();
                return;
            }

            IVariable selectedVariable = varData.VarRef;

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

            string[] options = orderedLabels.ToArray();
            int prevSelectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
            if (prevSelectedIndex < 0) prevSelectedIndex = 0;
            if (!shouldDrawLiteral) popupRect = fieldRect;

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUI.Popup(popupRect, prevSelectedIndex, options);
            bool popupChanged = EditorGUI.EndChangeCheck();

            if (popupChanged)
            {
                IVariable chosenNow = orderedVars[selectedIndex];
                bool choseLiteralValue = chosenNow == null;

                SerializedProperty owningFcProp = backingVarRefProp.FindPropertyRelative("legacyOwningFc");
                SerializedProperty owningVsaProp = backingVarRefProp.FindPropertyRelative("legacyOwningVsa");
                SerializedProperty ownerProp = backingVarRefProp.FindPropertyRelative("owningSource");

                if (choseLiteralValue)
                {
                    owningFcProp.objectReferenceValue = null;
                    owningVsaProp.objectReferenceValue = null;
                    ownerProp.objectReferenceValue = localFlowchart;
                    itemIdProp.intValue = Variable.InvalidID;
                }
                else
                {
                    var vOwner = chosenNow.Owner;

                    owningFcProp.objectReferenceValue = null;
                    owningVsaProp.objectReferenceValue = null;
                    ownerProp.objectReferenceValue = vOwner as UnityObj;
                    itemIdProp.intValue = chosenNow.ItemId;
                }
            }

            varData = varDataProp.boxedValue as VariableData;
            varData.Refresh();

            RestoreLayout();
            EditorGUI.EndProperty();

            varDataProp.serializedObject.ApplyModifiedProperties();
        }

    }
}