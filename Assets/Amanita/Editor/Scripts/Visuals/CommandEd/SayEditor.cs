using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Amanita.DialogueSys.VScripting;
using AtMycelia.Amanita.EditorUtils;
using AtMycelia.Amanita.DialogueSys;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    [CustomEditor (typeof(Say))]
    public class SayEditor : CommandEditor
    {
        public static bool showTagHelp;
        public Texture2D blackTex;
        
        public static void DrawTagHelpLabel()
        {
            string tagsText = TextTagParser.GetTagHelp();

            if (CustomTag.activeCustomTags.Count > 0)
            {
                tagsText += "\n\n\t-------- CUSTOM TAGS --------";
                List<Transform> activeCustomTagGroup = new List<Transform>();
                foreach (CustomTag ct in CustomTag.activeCustomTags)
                {
                    if(ct.transform.parent != null)
                    {
                        if (!activeCustomTagGroup.Contains(ct.transform.parent.transform))
                        {
                            activeCustomTagGroup.Add(ct.transform.parent.transform);
                        }
                    }
                    else
                    {
                        activeCustomTagGroup.Add(ct.transform);
                    }
                }
                foreach(Transform parent in activeCustomTagGroup)
                {
                    string tagName = parent.name;
                    string tagStartSymbol = "";
                    string tagEndSymbol = "";
                    CustomTag parentTag = parent.GetComponent<CustomTag>();
                    if (parentTag != null)
                    {
                        tagName = parentTag.name;
                        tagStartSymbol = parentTag.TagStartSymbol;
                        tagEndSymbol = parentTag.TagEndSymbol;
                    }
                    tagsText += "\n\n\t" + tagStartSymbol + " " + tagName + " " + tagEndSymbol;
                    foreach(Transform child in parent)
                    {
                        tagName = child.name;
                        tagStartSymbol = "";
                        tagEndSymbol = "";
                        CustomTag childTag = child.GetComponent<CustomTag>();
                        if (childTag != null)
                        {
                            tagName = childTag.name;
                            tagStartSymbol = childTag.TagStartSymbol;
                            tagEndSymbol = childTag.TagEndSymbol;
                        }
                            tagsText += "\n\t      " + tagStartSymbol + " " + tagName + " " + tagEndSymbol;
                    }
                }
            }
            tagsText += "\n";
            float pixelHeight = EditorStyles.miniLabel.CalcHeight(new GUIContent(tagsText), EditorGUIUtility.currentViewWidth);
            EditorGUILayout.SelectableLabel(tagsText, GUI.skin.GetStyle("HelpBox"), GUILayout.MinHeight(pixelHeight));
        }
        
        protected SerializedProperty characterProp;
        protected SerializedProperty portraitProp;
        protected SerializedProperty storyTextProp;
        protected SerializedProperty descriptionProp;
        protected SerializedProperty voiceOverClipProp;
        protected SerializedProperty showAlwaysProp;
        protected SerializedProperty showCountProp;
        protected SerializedProperty extendPreviousProp;
        protected SerializedProperty fadeWhenDoneProp;
        protected SerializedProperty waitForClickProp;
        protected SerializedProperty stopVoiceoverProp;
        protected SerializedProperty setSayDialogProp;
        protected SerializedProperty waitForVOProp;

        public override void OnEnable()
        {
            base.OnEnable();

            // Note that all these props are now for VariableData objects, so
            // they will be drawn with the appropriate VariableProperty
            // attribute handling (dropdowns for variables, fields for constants)
            characterProp = serializedObject.FindProperty("_character");
            portraitProp = serializedObject.FindProperty("_portrait");
            storyTextProp = serializedObject.FindProperty("_storyText");
            descriptionProp = serializedObject.FindProperty("_description");
            voiceOverClipProp = serializedObject.FindProperty("_voiceOverClip");
            showAlwaysProp = serializedObject.FindProperty("_showAlways");
            showCountProp = serializedObject.FindProperty("_showCount");
            extendPreviousProp = serializedObject.FindProperty("_extendPrevious");
            fadeWhenDoneProp = serializedObject.FindProperty("_fadeWhenDone");
            waitForClickProp = serializedObject.FindProperty("_waitForClick");
            stopVoiceoverProp = serializedObject.FindProperty("_stopVoiceover");
            setSayDialogProp = serializedObject.FindProperty("_setSayDialog");
            waitForVOProp = serializedObject.FindProperty("_waitForVO");

            if (blackTex == null)
            {
                blackTex = CustomGUI.CreateBlackTexture();
            }
        }
        
        protected virtual void OnDisable()
        {
            DestroyImmediate(blackTex);
        }

        public override void DrawCommandGUI() 
        {
            serializedObject.Update();

            bool showPortraits = false;
            EditorGUILayout.PropertyField(characterProp);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            EditorGUILayout.EndHorizontal();

            Say sayBeingDrawn = target as Say;

            bool characterIsSelected = sayBeingDrawn.Character != null;
            bool characterHasPortraitsField = characterIsSelected && 
                sayBeingDrawn.Character.Portraits != null;
            bool characterHasAtLeastOnePortrait = characterHasPortraitsField && 
                sayBeingDrawn.Character.Portraits.Count > 0;
            if (characterIsSelected && characterHasPortraitsField && characterHasAtLeastOnePortrait)  
            {
                showPortraits = true;    
            }

            if (showPortraits) 
            {
                ObjectField(portraitProp, _portraitLabelContent, 
                    _noneGuiContent, sayBeingDrawn.Character.Portraits);
            }
            else
            {
                if (!sayBeingDrawn.ExtendPrevious)
                {
                    sayBeingDrawn.Portrait = null;
                }
            }

            HandleTagHelpLabel();
            static void HandleTagHelpLabel()
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                bool clickedTagHelpButton = GUILayout.Button(_tagHelpContent, _tagHelpStyle);
                if (clickedTagHelpButton)
                {
                    showTagHelp = !showTagHelp;
                }
                EditorGUILayout.EndHorizontal();

                if (showTagHelp)
                {
                    DrawTagHelpLabel();
                    EditorGUILayout.Separator();
                }

            }

            EditorGUILayout.PropertyField(storyTextProp);

            EditorGUILayout.PropertyField(descriptionProp);
            EditorGUILayout.PropertyField(extendPreviousProp);

            EditorGUILayout.PropertyField(voiceOverClipProp, _voiceClipLabelContent);

            EditorGUILayout.PropertyField(showAlwaysProp);
            
            if (showAlwaysProp.boolValue == false)
            {
                EditorGUILayout.PropertyField(showCountProp);
            }

            GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
            GUIStyle leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButton.fontSize = 10;
            leftButton.font = EditorStyles.toolbarButton.font;
            GUIStyle rightButton = new GUIStyle(EditorStyles.miniButtonRight);
            rightButton.fontSize = 10;
            rightButton.font = EditorStyles.toolbarButton.font;

            EditorGUILayout.PropertyField(fadeWhenDoneProp);
            EditorGUILayout.PropertyField(waitForClickProp);
            EditorGUILayout.PropertyField(stopVoiceoverProp);
            EditorGUILayout.PropertyField(setSayDialogProp);
            EditorGUILayout.PropertyField(waitForVOProp);
            
            if (showPortraits && sayBeingDrawn.Portrait != null)
            {
                Texture2D characterTexture = sayBeingDrawn.Portrait.texture;
                float aspect = (float)characterTexture.width / characterTexture.height;
                Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), 
                    GUILayout.ExpandWidth(true));
                if (characterTexture != null)
                {
                    GUI.DrawTexture(previewRect, characterTexture, ScaleMode.ScaleToFit, true, aspect);
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private static readonly GUIContent _tagHelpContent = new GUIContent("Tag Help", "View available tags");
        private static readonly GUIStyle _tagHelpStyle = new GUIStyle(EditorStyles.miniButton);

        private static readonly GUIContent _portraitLabelContent = new GUIContent("Portrait", 
            "Portrait representing speaking character");
        private static readonly GUIContent _noneGuiContent = new GUIContent("<None>");
        private static readonly GUIContent _voiceClipLabelContent = new GUIContent("Voice Over Clip", 
            "Voice over audio to play when the text is displayed");

    }    
}
