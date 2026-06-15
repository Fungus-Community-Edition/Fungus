using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using AtMycelia.Amanita;
using AtMycelia.Amanita.DialogueSys.VScripting;
using AtMycelia.Amanita.DialogueSys;

namespace AtMycelia.Hyphlow.EditorExt
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
        
        public override void OnEnable()
        {
            base.OnEnable();

            FetchProps();
            void FetchProps()
            {
                // Note that all these props are now for VariableData objects, so
                // they will be drawn with the appropriate VariableProperty
                // attribute handling (dropdowns for variables, fields for constants)

                _characterProp = serializedObject.FindProperty("_character");
                _portraitProp = serializedObject.FindProperty("_portrait");
                _storyTextProp = serializedObject.FindProperty("_storyText");
                _descriptionProp = serializedObject.FindProperty("_description");
                _voiceOverClipProp = serializedObject.FindProperty("_voiceOverClip");
                _showAlwaysProp = serializedObject.FindProperty("_showAlways");
                _showCountProp = serializedObject.FindProperty("_showCount");
                _extendPreviousProp = serializedObject.FindProperty("_extendPrevious");
                _fadeWhenDoneProp = serializedObject.FindProperty("_fadeWhenDone");
                _waitForClickProp = serializedObject.FindProperty("_waitForClick");
                _stopVoiceoverProp = serializedObject.FindProperty("_stopVoiceover");
                _setSayDialogProp = serializedObject.FindProperty("_setSayDialog");
                _waitForVOProp = serializedObject.FindProperty("_waitForVO");
            }

            if (blackTex == null)
            {
                blackTex = CustomGUI.CreateBlackTexture();
            }

        }

        protected SerializedProperty _characterProp;
        protected SerializedProperty _portraitProp;
        protected SerializedProperty _storyTextProp;
        protected SerializedProperty _descriptionProp;
        protected SerializedProperty _voiceOverClipProp;
        protected SerializedProperty _showAlwaysProp;
        protected SerializedProperty _showCountProp;
        protected SerializedProperty _extendPreviousProp;
        protected SerializedProperty _fadeWhenDoneProp;
        protected SerializedProperty _waitForClickProp;
        protected SerializedProperty _stopVoiceoverProp;
        protected SerializedProperty _setSayDialogProp;
        protected SerializedProperty _waitForVOProp;

        private static GUIContent _tagHelpContent;
        private static GUIStyle _tagHelpStyle;

        private static GUIContent _portraitLabelContent;
        private static GUIContent _noneGuiContent;
        private static GUIContent _voiceClipLabelContent;

        protected virtual void OnDisable()
        {
            DestroyImmediate(blackTex);
        }

        public override void DrawCommandGUI() 
        {
            serializedObject.Update();
            UpdateGuiContentMembers();

            bool showPortraits = false;
            EditorGUILayout.PropertyField(_characterProp);

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
                DrawPortraitField(_portraitProp, _portraitLabelContent, 
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

            EditorGUILayout.PropertyField(_storyTextProp);

            EditorGUILayout.PropertyField(_descriptionProp);
            EditorGUILayout.PropertyField(_extendPreviousProp);

            EditorGUILayout.PropertyField(_voiceOverClipProp, _voiceClipLabelContent);

            EditorGUILayout.PropertyField(_showAlwaysProp);
            BooleanData showAlwaysData = _showAlwaysProp.boxedValue as BooleanData;
            if (!showAlwaysData.Value)
            {
                EditorGUILayout.PropertyField(_showCountProp);
            }

            GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
            centeredLabel.alignment = TextAnchor.MiddleCenter;
            GUIStyle leftButton = new GUIStyle(EditorStyles.miniButtonLeft);
            leftButton.fontSize = 10;
            leftButton.font = EditorStyles.toolbarButton.font;
            GUIStyle rightButton = new GUIStyle(EditorStyles.miniButtonRight);
            rightButton.fontSize = 10;
            rightButton.font = EditorStyles.toolbarButton.font;

            EditorGUILayout.PropertyField(_fadeWhenDoneProp);
            EditorGUILayout.PropertyField(_waitForClickProp);
            EditorGUILayout.PropertyField(_stopVoiceoverProp);
            EditorGUILayout.PropertyField(_setSayDialogProp);
            EditorGUILayout.PropertyField(_waitForVOProp);
            
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

        private static void UpdateGuiContentMembers()
        {
            _tagHelpContent ??= new GUIContent("Tag Help", "View available tags");
            _tagHelpStyle ??= new GUIStyle(EditorStyles.miniButton);

            _portraitLabelContent ??= new GUIContent("Portrait",
            "Portrait representing speaking character");
            _noneGuiContent ??= new GUIContent("<None>");
            _voiceClipLabelContent ??= new GUIContent("Voice Over Clip",
            "Voice over audio to play when the text is displayed");
        }

        private static void DrawPortraitField(SerializedProperty portraitProperty, 
            GUIContent label, GUIContent nullLabel, List<Sprite> portraits)
        {
            if (portraitProperty == null)
            {
                return;
            }

            SerializedProperty literalValueProperty = portraitProperty.FindPropertyRelative("_value");
            if (literalValueProperty == null || 
                literalValueProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                Debug.LogError("Error: Could not find expected literal value property " +
                    "for portrait field, drawing default object field");
                ObjectField(portraitProperty, label, nullLabel, portraits);
                return;
            }

            List<GUIContent> objectNames = new List<GUIContent>();
            Sprite selectedSprite = literalValueProperty.objectReferenceValue as Sprite;

            int selectedIndex = -1;
            objectNames.Add(nullLabel);
            if (selectedSprite == null)
            {
                selectedIndex = 0;
            }

            for (int i = 0; i < portraits.Count; ++i)
            {
                if (portraits[i] == null)
                {
                    continue;
                }
                objectNames.Add(new GUIContent(portraits[i].name));

                if (selectedSprite == portraits[i])
                {
                    selectedIndex = i + 1;
                }
            }

            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, objectNames.ToArray());

            if (selectedIndex == -1)
            {
                return;
            }

            Sprite result = selectedIndex == 0 ? null : portraits[selectedIndex - 1];
            if (selectedSprite == result)
            {
                return;
            }

            literalValueProperty.objectReferenceValue = result;
            ClearPortraitVariableReference(portraitProperty);
        }

        private static void ClearPortraitVariableReference(SerializedProperty portraitProperty)
        {
            SerializedProperty backingVarRefProperty = 
                portraitProperty.FindPropertyRelative("_backingVarRef");
            if (backingVarRefProperty == null)
            {
                return;
            }

            SerializedProperty itemIdProperty = backingVarRefProperty.FindPropertyRelative("_itemId");
            if (itemIdProperty != null)
            {
                itemIdProperty.intValue = 0;
            }

            SetObjectReferenceToNull(backingVarRefProperty, "_owningSource");
            SetObjectReferenceToNull(backingVarRefProperty, "_legacyOwningFc");
            SetObjectReferenceToNull(backingVarRefProperty, "_legacyOwningVsa");

            SerializedProperty legacyVarRefProperty = 
                portraitProperty.FindPropertyRelative("spriteRef");
            if (legacyVarRefProperty != null)
            {
                legacyVarRefProperty.objectReferenceValue = null;
            }
        }

        private static void SetObjectReferenceToNull(SerializedProperty parentProperty, 
            string childPropertyName)
        {
            SerializedProperty childProperty = parentProperty.FindPropertyRelative(childPropertyName);
            if (childProperty != null)
            {
                childProperty.objectReferenceValue = null;
            }
        }

    }    
}
