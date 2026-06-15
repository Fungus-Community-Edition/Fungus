using UnityEditor;
using UnityEngine;
using AmanitaMenu = AtMycelia.Amanita.DialogueSys.VScripting.Menu;
using AtMycelia.Hyphlow.EditorExt;
using AtMycelia.Hyphlow;

namespace AtMycelia.Amaniphlow.EditorExt
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

            // Updated to the new VariableData-backed field names used by Menu
            textProp = serializedObject.FindProperty("_text");
            descriptionProp = serializedObject.FindProperty("_description");
            targetBlockProp = serializedObject.FindProperty("_targetBlock");
            hideIfVisitedProp = serializedObject.FindProperty("_hideIfVisited");
            interactableProp = serializedObject.FindProperty("_interactable");
            setMenuDialogProp = serializedObject.FindProperty("_setMenuDialog");
            hideThisOptionProp = serializedObject.FindProperty("_hideThisOption");
        }
        
        public override void DrawCommandGUI()
        {
            var flowchart = EditorSelectionTracker.ActiveFlowchart;
            if (flowchart == null)
            {
                return;
            }
            
            serializedObject.Update();
            
            // VariableData fields (e.g. StringData, BooleanData) are serialized objects;
            // showing the property will expose the value/variable fields as appropriate.
            EditorGUILayout.PropertyField(textProp);
            EditorGUILayout.PropertyField(descriptionProp);

            EditorGUILayout.BeginHorizontal();
            // Draw the BlockReference using the existing BlockField helper.
            EditorGUILayout.PropertyField(targetBlockProp, new GUIContent("Target Block"));

            BlockReference blockRef = targetBlockProp != null ? 
                targetBlockProp.boxedValue as BlockReference: 
                null;
            IBlock blockTargeted = blockRef.Block;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(hideIfVisitedProp);
            EditorGUILayout.PropertyField(interactableProp);
            EditorGUILayout.PropertyField(setMenuDialogProp);
            EditorGUILayout.PropertyField(hideThisOptionProp);
            
            serializedObject.ApplyModifiedProperties();
        }
    }    
}
