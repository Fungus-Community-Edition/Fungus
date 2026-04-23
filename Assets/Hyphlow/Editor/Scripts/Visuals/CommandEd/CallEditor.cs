using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor (typeof(Call))]
    public class CallEditor : CommandEditor 
    {
        protected SerializedProperty targetFlowchartProp;
        protected SerializedProperty targetBlockProp;
        protected SerializedProperty startLabelProp;
        protected SerializedProperty startIndexProp;
        protected SerializedProperty callModeProp;

        public override void OnEnable()
        {
            base.OnEnable();

            targetFlowchartProp = serializedObject.FindProperty("_targetFlowchart");
            targetBlockProp = serializedObject.FindProperty("_targetBlock");
            startLabelProp = serializedObject.FindProperty("_startLabel");
            startIndexProp = serializedObject.FindProperty("_startIndex");
            callModeProp = serializedObject.FindProperty("_callMode");
        }

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            Call t = target as Call;

            Flowchart flowchart = null;
            if (targetFlowchartProp.objectReferenceValue == null)
            {
                flowchart = (Flowchart)t.GetFlowchart();
            }
            else
            {
                flowchart = targetFlowchartProp.objectReferenceValue as Flowchart;
            }

            EditorGUILayout.PropertyField(targetFlowchartProp);

            if (flowchart != null)
            {
                BlockEditor.BlockField(targetBlockProp,
                                       new GUIContent("Target Block", "Block to call"), 
                                       new GUIContent("<None>"), 
                                       flowchart);

                EditorGUILayout.PropertyField(startLabelProp);

                EditorGUILayout.PropertyField(startIndexProp);
            }

            EditorGUILayout.PropertyField(callModeProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
