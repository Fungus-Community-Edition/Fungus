using UnityEditor;
using UnityEngine;
using Amanita.DialogueSys.Commands;
using Amanita.DialogueSys;

namespace Amanita.VScripting.EditorUtils
{
    [CustomEditor (typeof(ControlStage))]
    public class StageEditor : CommandEditor
    {
        protected SerializedProperty displayProp;
        protected SerializedProperty stageProp;
        protected SerializedProperty replacedStageProp;
        protected SerializedProperty useDefaultSettingsProp;
        protected SerializedProperty fadeDurationProp;
        protected SerializedProperty waitUntilFinishedProp;

        public override void OnEnable()
        {
            base.OnEnable();

            displayProp = serializedObject.FindProperty("display");
            stageProp = serializedObject.FindProperty("stage");
            replacedStageProp = serializedObject.FindProperty("replacedStage");
            useDefaultSettingsProp = serializedObject.FindProperty("useDefaultSettings");
            fadeDurationProp = serializedObject.FindProperty("fadeDuration");
            waitUntilFinishedProp = serializedObject.FindProperty("waitUntilFinished");
        }
        
        public override void DrawCommandGUI() 
        {
            serializedObject.Update();
            
            ControlStage stageTarget = target as ControlStage;

            // Format Enum names
            string[] displayLabels = StringFormatter.FormatEnumNames(stageTarget.Display,"<None>");
            displayProp.enumValueIndex = EditorGUILayout.Popup("Display", (int)displayProp.enumValueIndex, displayLabels);

            string replaceLabel = "Portrait Stage";
            if (stageTarget.Display == StageDisplayType.Swap)
            {
                CommandEditor.ObjectField<Stage>(replacedStageProp, 
                                                 new GUIContent("Replace", "Character to swap with"), 
                                                 new GUIContent("<Default>"),
                                                 Stage.ActiveStages);
                replaceLabel = "With";
            }

            if (Stage.ActiveStages.Count > 0)
            {
                CommandEditor.ObjectField<Stage>(stageProp, 
                                                 new GUIContent(replaceLabel, "Stage to display the character portraits on"), 
                                                 new GUIContent("<Default>"),
                                                 Stage.ActiveStages);
            }

            bool showOptionalFields = true;
            Stage stageToWorkWith = stageTarget._Stage;
            // Only show optional portrait fields once required fields have been filled...
            if (stageTarget._Stage != null)                // Character is selected
            {
                if (stageTarget._Stage == null)        // If no default specified, try to get any portrait stage in the scene
                {
                #if UNITY_6000
                    stageToWorkWith = GameObject.FindFirstObjectByType<Stage>();
                #else
                    s = GameObject.FindObjectOfType<Stage>();
                #endif
                }
                if (stageToWorkWith == null)
                {
                    EditorGUILayout.HelpBox("No portrait stage has been set.", MessageType.Error);
                    showOptionalFields = false; 
                }
            }
            if (stageTarget.Display != StageDisplayType.None && showOptionalFields) 
            {
                EditorGUILayout.PropertyField(useDefaultSettingsProp);
                if (!stageTarget.UseDefaultSettings)
                {
                    EditorGUILayout.PropertyField(fadeDurationProp);
                }
                EditorGUILayout.PropertyField(waitUntilFinishedProp);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
