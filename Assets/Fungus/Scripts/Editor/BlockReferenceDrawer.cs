// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEditor;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Custom drawer for the BlockReference, allows for more easily selecting a target block in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(Amanita.BlockReference))]
    public class BlockReferenceDrawer : PropertyDrawer
    {
        public Amanita.Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var l = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, l);
            position.height = EditorGUIUtility.singleLineHeight;
            var block = property.FindPropertyRelative("block");

            Amanita.Block b = block.objectReferenceValue as Amanita.Block;

            if (block.objectReferenceValue != null && lastFlowchart == null)
            {
                if (b != null)
                {
                    lastFlowchart = b.GetFlowchart();
                }
            }

            lastFlowchart = EditorGUI.ObjectField(position, lastFlowchart, typeof(Amanita.Flowchart), true) as Amanita.Flowchart;
            position.y += EditorGUIUtility.singleLineHeight;
            if (lastFlowchart != null)
                b = Amanita.EditorUtils.BlockEditor.BlockField(position, new GUIContent("None"), lastFlowchart, b);
            else
                EditorGUI.PrefixLabel(position, new GUIContent("Flowchart Required"));

            block.objectReferenceValue = b;

            block.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
}