using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Type = System.Type;
using UnityObj = UnityEngine.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public abstract class VariableDataDrawerBase : PropertyDrawer
    {
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
            AnyVariableData anyVarData = varData as AnyVariableData; // We want to handle AnyVariableData as a special case
            if (varData == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            SerializedProperty literalValueProp, backingVarRefProp = null, itemIdProp = null;
            FetchProps(out bool shouldContinue);
            void FetchProps(out bool shouldContinue)
            {
                shouldContinue = true;
                literalValueProp = varDataProp.FindPropertyRelative("_value");
                if (anyVarData != null)
                {
                    literalValueProp = varDataProp.FindPropertyRelative("_data._value");
                }
                backingVarRefProp = varDataProp.FindPropertyRelative("_backingVarRef");
                if (anyVarData != null)
                {
                    backingVarRefProp = varDataProp.FindPropertyRelative("_data._backingVarRef");
                }
                if (backingVarRefProp == null)
                {
                    shouldContinue = false;
                    EditorGUI.EndProperty();
                    return;
                }

                itemIdProp = backingVarRefProp.FindPropertyRelative("_itemId");
            }

            if (!shouldContinue)
            {
                return;
            }

            bool shouldDrawLiteral = ShouldDrawLiteral(varDataProp);
            Rect labelRect, valueRect, popupRect, fieldRect;
            int prevIndent;
            HandleLayout();
            void HandleLayout()
            {
                float labelWidth = EditorGUIUtility.labelWidth;
                float labelOffset = (EditorGUI.indentLevel * 15f);

                bool useMultilineLabel = UseMultilineLabel(varDataProp, varData, shouldDrawLiteral);

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
                    SerializedProperty owningFcProp = backingVarRefProp.FindPropertyRelative("_owningSource");
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
                warningMessage = $"Could not resolve ContentType for {GetType().Name} " +
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
                IReadOnlyDictionary<string, IVariable> validVars = GetValidVariables(varDataProp, fieldInfo);
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

                SerializedProperty ownerProp = backingVarRefProp.FindPropertyRelative("_owningSource");

                if (choseLiteralValue)
                {
                    ownerProp.objectReferenceValue = localFlowchart;
                    itemIdProp.intValue = Variable.InvalidID;
                }
                else
                {
                    var vOwner = chosenNow.Owner;
                    anyVarData = varDataProp.boxedValue as AnyVariableData;

                    if (anyVarData != null)
                    {
                        bool sameContentType = chosenNow.ContentType.Equals(anyVarData.ContentType);
                        anyVarData.SetFor(chosenNow.ContentType);
                        anyVarData.VarRef = chosenNow;
                        varDataProp.boxedValue = anyVarData;

                        if (!sameContentType)
                        {
                            // This means that the underlying VariableData changed to a whole new instance. 
                            // Thus, we'll need to refetch the properties to point to the new instance.
                            backingVarRefProp = varDataProp.FindPropertyRelative("_data._backingVarRef");
                            ownerProp = backingVarRefProp.FindPropertyRelative("_owningSource");
                            itemIdProp = backingVarRefProp.FindPropertyRelative("_itemId");
                        }
                    }

                    ownerProp.objectReferenceValue = vOwner as UnityObj;
                    itemIdProp.intValue = chosenNow.ItemId;
                }
            }

            RestoreLayout();
            EditorGUI.EndProperty();

            varDataProp.serializedObject.ApplyModifiedProperties();
        }

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

            if (UseMultilineLabel(varDataProp, varData, true))
            {
                return baseHeight + EditorGUIUtility.standardVerticalSpacing + textAreaHeight;
            }

            return textAreaHeight;
        }


        protected virtual bool UseMultilineLabel(SerializedProperty varDataProp, VariableData varData, bool shouldDrawLiteral)
        {
            return false;
        }

        protected virtual bool ShouldUseTextArea(VariableData varData)
        {
            if (varData is not StringData)
            {
                return false;
            }

            return GetTextAreaAttribute() != null;
        }

        protected bool ShouldWordWrapTextArea()
        {
            HyphlowTextAreaAttribute textAreaAttribute = GetTextAreaAttribute();
            if (textAreaAttribute == null)
            {
                return false;
            }

            int lineCount = Mathf.Max(1, textAreaAttribute.MinLines);
            return lineCount >= 2;
        }

        protected HyphlowTextAreaAttribute GetTextAreaAttribute()
        {
            return fieldInfo?.GetCustomAttribute<HyphlowTextAreaAttribute>();
        }

        protected static bool ShouldDrawLiteral(SerializedProperty varDataProp)
        {
            var varData = varDataProp.boxedValue as VariableData;
            if (varData != null)
            {
                return !varData.RepresentingVar;
            }

            var backingVarRefProp = varDataProp.FindPropertyRelative("_backingVarRef");
            var itemIdProp = backingVarRefProp?.FindPropertyRelative("_itemId");
            return itemIdProp == null || itemIdProp.intValue == Variable.InvalidID;
        }

        protected static readonly int popupWidth = Mathf.RoundToInt(EditorGUIUtility.singleLineHeight);
        protected static readonly int popupGap = 5;
        protected static int SpaceForPopup => popupWidth + popupGap;
        protected static readonly float MinimumValueWidth = 80f;
        protected static VariableRegistry VarRegistry => VariableRegistryService.Registry;

        private static IReadOnlyDictionary<string, IVariable> GetValidVariables(SerializedProperty varDataProp, 
            FieldInfo fieldInfo)
        {
            IReadOnlyDictionary<string, IVariable> validVars;
            IVariableData varData = varDataProp.boxedValue as IVariableData;
            if (varData == null)
            {
                Debug.LogError($"Could not get IVariableData from property drawer for {varDataProp.propertyPath}.");
                validVars = new Dictionary<string, IVariable>();
                return validVars;
            }

            if (varData is AnyVariableData)
            {
                var allowedTypes = GetAllowedTypes(fieldInfo);
                if (allowedTypes != null && allowedTypes.Length > 0)
                {
                    validVars = VarRegistry.GetVarsOfMultiTypes(allowedTypes, true);
                }
                else
                {
                    validVars = VarRegistry.GetVarsOfType(typeof(object), true);
                }
            }
            else
            {
                Type contentType = varData.ContentType;
                if (contentType != null)
                {
                    validVars = VarRegistry.GetVarsOfType(contentType, true);
                }
                else
                {
                    Debug.LogError($"ContentType was null for variable data at {varDataProp.propertyPath}. " +
                        $"Cannot determine valid variables to show in dropdown.");
                    validVars = new Dictionary<string, IVariable>();
                }

            }

            return validVars;
        }

        protected static Type[] GetAllowedTypes(FieldInfo fieldInfo)
        {
            Type[] result = null;
            var attr = fieldInfo.GetCustomAttribute<ContentTypeConstraintAttribute>();
            if (attr == null)
            {
                result = Array.Empty<Type>();
            }
            else if (attr.AllowedTypes.Count == 0)
            {
                result = new Type[1] { typeof(object) };
            }
            else if (attr.AllowedTypes.Count > 0)
            {
                result = attr.AllowedTypes.ToArray();
            }
            return result;
        }


    }

    // For the fields that can accept either a variable or a literal value
    [CustomPropertyDrawer(typeof(VariableData), true)]
    public class VariableDataDrawer : VariableDataDrawerBase
    {
    }

    
}