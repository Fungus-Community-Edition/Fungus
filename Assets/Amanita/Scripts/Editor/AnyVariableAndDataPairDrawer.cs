using System;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Custom drawer for the AnyVaraibleAndDataPair, shows only the matching data for the targeted variable
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(AnyVariableAndDataPair))]
    public class AnyVariableAndDataPairDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty varProp = property.FindPropertyRelative("variable");
            EditorGUI.PropertyField(position, varProp, label);

            position.y += EditorGUIUtility.singleLineHeight;
            SerializedProperty dataProp = property.FindPropertyRelative("data.data");

            if (varProp.objectReferenceValue != null && dataProp != null)
            {
                // Let Unity's property drawer system handle drawing the data
                Debug.Log("Letting Unity's property drawer system handle drawing the data");
                EditorGUI.PropertyField(position, dataProp, new GUIContent("Data"));
            }
            else
            {
                EditorGUI.LabelField(position, "Must select a variable before setting data.");
            }

            GUILayout.Space(20);
            property.serializedObject.ApplyModifiedProperties();
        }

        protected static bool TryGetTypeActionsFor(System.Type varPropType, out VariableTypeActions typeActionsRes)
        {
            return VariableTypeRegistry.TryGetTypeActionsFor(varPropType, out typeActionsRes);
        }

    }
}