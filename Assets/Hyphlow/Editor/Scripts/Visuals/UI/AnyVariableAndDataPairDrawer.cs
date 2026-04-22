using System;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Custom drawer for the AnyVaraibleAndDataPair, shows only the matching data for the targeted variable
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnyVariableAndDataPair))]
    public class AnyVariableAndDataPairDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty holdsVarAndDataPair, GUIContent label)
        {
            SerializedProperty lhsVarRefProp;
            DisplayLeftHandSideVar();
            void DisplayLeftHandSideVar()
            {
                lhsVarRefProp = holdsVarAndDataPair.FindPropertyRelative("_varRef");
                EditorGUI.PropertyField(position, lhsVarRefProp, label);
                lhsVarRefProp.serializedObject.ApplyModifiedProperties();
            }

            AnyVariableAndDataPair pairInstance = holdsVarAndDataPair.boxedValue as AnyVariableAndDataPair;
            IVariable currentLeftHandSideVar = pairInstance.LhsVariable;

            AnyVariableData anyVarData = pairInstance.Data;
            position.y += EditorGUIUtility.singleLineHeight;

            HandleInnerDataField();
            void HandleInnerDataField()
            {
                var effectiveVarType = GetEffectiveVarType(currentLeftHandSideVar);
                if (effectiveVarType == null)
                {
                    EditorGUI.LabelField(position, "Must select a variable before setting data.");
                    return;
                }
                anyVarData.SetFor(effectiveVarType, currentLeftHandSideVar.ContentType);

                HandleLhsVarChanges();
                void HandleLhsVarChanges()
                {
                    bool lhsVarChanged = !ReferenceEquals(_prevLeftHandSideVar, currentLeftHandSideVar);
                    bool validAnyVarData = anyVarData != null;
                    if (lhsVarChanged && validAnyVarData && currentLeftHandSideVar != null)
                    {
                        _prevLeftHandSideVar = currentLeftHandSideVar;
                        pairInstance.LhsVariable = currentLeftHandSideVar;
                    }
                }

                holdsVarAndDataPair.boxedValue = pairInstance;
                holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();

                DrawInnerDataField();
                void DrawInnerDataField()
                {
                    SerializedProperty innerDataProp = holdsVarAndDataPair.FindPropertyRelative("_data._data");
                    if (currentLeftHandSideVar != null && innerDataProp != null)
                    {
                        EditorGUI.PropertyField(position, innerDataProp, new GUIContent("Data"), includeChildren: true);
                    }
                    else
                    {
                        EditorGUI.LabelField(position, "Must select a variable before setting data.");
                    }
                }
            }

            GUILayout.Space(20);
            holdsVarAndDataPair.serializedObject.ApplyModifiedProperties();
        }

        private static Type GetEffectiveVarType(IVariable var)
        {
            return var?.GetType();
        }

        protected IVariable _prevLeftHandSideVar;

        protected static bool TryGetTypeActionsFor(Type varPropType, out VariableTypeActions typeActionsRes)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varPropType, out typeActionsRes);
        }
    }
}