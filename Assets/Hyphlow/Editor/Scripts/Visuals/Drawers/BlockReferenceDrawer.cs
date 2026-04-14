using UnityEngine;
using UnityEditor;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Custom drawer for the BlockReference, allows for more easily selecting a target block in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(BlockReference))]
    public class BlockReferenceDrawer : PropertyDrawer
    {
        public Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var l = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, l);
            position.height = EditorGUIUtility.singleLineHeight;
            var block = property.FindPropertyRelative("block");

            Block b = block.objectReferenceValue as Block;

            if (block.objectReferenceValue != null && lastFlowchart == null)
            {
                if (b != null)
                {
                    lastFlowchart = b.GetFlowchart();
                }
            }

            lastFlowchart = EditorGUI.ObjectField(position, lastFlowchart, typeof(Flowchart), true) as Flowchart;
            position.y += EditorGUIUtility.singleLineHeight;
            if (lastFlowchart != null)
                b = BlockEditor.BlockField(position, new GUIContent("None"), lastFlowchart, b);
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