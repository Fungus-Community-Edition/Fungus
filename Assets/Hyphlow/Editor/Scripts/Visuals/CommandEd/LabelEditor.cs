using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor (typeof(Label))]
    public class LabelEditor : CommandEditor
    {
        protected SerializedProperty keyProp;
        
        public static void LabelField(SerializedProperty property, 
                                      GUIContent labelText, 
                                      Block block)
        {
            List<string> labelKeys = new List<string>();
            List<Label> labelObjects = new List<Label>();
            
            labelKeys.Add("<None>");
            labelObjects.Add(null);
            
            Label selectedLabel = property.objectReferenceValue as Label;

            int index = 0;
            int selectedIndex = 0;
            var commandList = block.CommandList;
            foreach (var command in commandList)
            {
                Label label = command as Label;
                if (label == null)
                {
                    continue;
                }

                labelKeys.Add(label.Key);
                labelObjects.Add(label);
                
                index++;
                
                if (label == selectedLabel)
                {
                    selectedIndex = index;
                }
            }

            selectedIndex = EditorGUILayout.Popup(labelText.text, selectedIndex, labelKeys.ToArray());

            property.objectReferenceValue = labelObjects[selectedIndex];
        }

        public override void OnEnable()
        {
            base.OnEnable();

            keyProp = serializedObject.FindProperty("_key");
        }
        
        public override void DrawCommandGUI()
        {
            Label t = target as Label;

            var flowchart = t.GetFlowchart();
            if (flowchart == null)
            {
                return;
            }
        
            serializedObject.Update();

            EditorGUILayout.PropertyField(keyProp);
            keyProp.stringValue = flowchart.GetUniqueLabelKey(keyProp.stringValue, t);

            serializedObject.ApplyModifiedProperties();
        }
    }    
}
