using UnityEditor;
using UnityEngine;
using AmanitaMenu = AtMycelia.Amanita.DialogueSys.Commands.Menu;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    [CustomEditor (typeof(AmanitaMenu))]
    public class MenuEditor : CommandEditor 
    {
        protected SerializedProperty textProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty targetBlockProp;
        protected SerializedProperty hideIfVisitedProp;
        protected SerializedProperty interactableProp;
        protected SerializedProperty setMenuDialogProp;
        protected SerializedProperty hideThisOptionProp;

        public override void OnEnable()
        {
            base.OnEnable();

            textProp = serializedObject.FindProperty("text");
            descriptionProp = serializedObject.FindProperty("description");
            targetBlockProp = serializedObject.FindProperty("targetBlock");
            hideIfVisitedProp = serializedObject.FindProperty("hideIfVisited");
            interactableProp = serializedObject.FindProperty("interactable");
            setMenuDialogProp = serializedObject.FindProperty("setMenuDialog");
            hideThisOptionProp = serializedObject.FindProperty("hideThisOption");
        }
        
        public override void DrawCommandGUI()
        {
            var flowchart = EditorSelectionTracker.ActiveFlowchart;
            if (flowchart == null)
            {
                return;
            }
            
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(textProp);

            EditorGUILayout.PropertyField(descriptionProp);

            EditorGUILayout.BeginHorizontal();
            BlockEditor.BlockField(targetBlockProp,
                                   new GUIContent("Target Block", "Block to call when option is selected"), 
                                   new GUIContent("<None>"), 
                                   flowchart);
            const int popupWidth = 17;
            if (targetBlockProp.objectReferenceValue == null && GUILayout.Button("+",GUILayout.MaxWidth(popupWidth)))
            {
                var menuTarget = (AmanitaMenu)target;
                var activeFlowchart = EditorSelectionTracker.ActiveFlowchart;
                Vector2 pos = menuTarget.ParentBlock._NodeRect.position - Vector2.down * 60;
                var newBlock = activeFlowchart.CreateBlock(pos);
                targetBlockProp.objectReferenceValue = newBlock;
                activeFlowchart.SelectedBlock = menuTarget.ParentBlock;
            }
            EditorGUILayout.EndHorizontal();



            EditorGUILayout.PropertyField(hideIfVisitedProp);
            EditorGUILayout.PropertyField(interactableProp);
            EditorGUILayout.PropertyField(setMenuDialogProp);
            EditorGUILayout.PropertyField(hideThisOptionProp);
            
            serializedObject.ApplyModifiedProperties();
        }
    }    
}
