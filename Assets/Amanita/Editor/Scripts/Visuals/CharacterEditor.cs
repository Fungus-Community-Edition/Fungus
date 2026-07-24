using UnityEditor;
using UnityEngine;

namespace AtMycelia.Amanita.EditorExt
{
    [CustomEditor (typeof(Character))]
    public class CharacterEditor : Editor
    {
        protected virtual void OnEnable()
        {
            _nameTextProp = serializedObject.FindProperty ("_nameText");
            _nameColorProp = serializedObject.FindProperty ("_nameColor");
            _soundEffectProp = serializedObject.FindProperty ("_soundEffect");
            _portraitsProp = serializedObject.FindProperty ("_portraits");
            _portraitsFaceProp = serializedObject.FindProperty ("_portraitsFace");
            _descriptionProp = serializedObject.FindProperty ("_description");
            _setSayDialogProp = serializedObject.FindProperty("_setSayDialog");
            _effectAudioSourceProp = serializedObject.FindProperty("_effectAudioSource");
            _voiceAudioSourceProp = serializedObject.FindProperty("_voiceAudioSource");
        }

        protected SerializedProperty _nameTextProp;
        protected SerializedProperty _nameColorProp;
        protected SerializedProperty _soundEffectProp;
        protected SerializedProperty _portraitsProp;
        protected SerializedProperty _portraitsFaceProp;
        protected SerializedProperty _descriptionProp;
        protected SerializedProperty _setSayDialogProp;
        protected SerializedProperty _effectAudioSourceProp;
        protected SerializedProperty _voiceAudioSourceProp;

        public override void OnInspectorGUI() 
        {
            serializedObject.Update();

            Character chara = target as Character;
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_nameTextProp, 
                new GUIContent("Name Text", "Name of the character display in the dialog"));
            EditorGUILayout.PropertyField(_nameColorProp, 
                new GUIContent("Name Color", "Color of name text display in the dialog"));
            EditorGUILayout.PropertyField(_soundEffectProp,
                new GUIContent("Sound Effect", "Sound to play when the character is talking. " +
                "Overrides the setting in the Dialog."));
            EditorGUILayout.PropertyField(_effectAudioSourceProp);
            EditorGUILayout.PropertyField(_voiceAudioSourceProp);
            EditorGUILayout.PropertyField(_setSayDialogProp);
            EditorGUILayout.PropertyField(_descriptionProp, 
                new GUIContent("Description", "Notes about this story character (personality, " +
                "attibutes, etc.)"));

            if (chara.Portraits != null &&
                chara.Portraits.Count > 0)
            {
                chara.ProfileSprite = chara.Portraits[0];
            }
            else
            {
                chara.ProfileSprite = null;
            }
            
            if (chara.ProfileSprite != null)
            {
                Texture2D characterTexture = chara.ProfileSprite.texture;
                float aspect = characterTexture.width / characterTexture.height;
                Rect previewRect = GUILayoutUtility.GetAspectRect(aspect, GUILayout.Width(100), 
                    GUILayout.ExpandWidth(true));

                if (characterTexture != null)
                {
                    GUI.DrawTexture(previewRect, characterTexture, ScaleMode.ScaleToFit, true, aspect);
                }
            }

            EditorGUILayout.PropertyField(_portraitsProp, new GUIContent("Portraits", 
                "Character image sprites to display in the dialog"), true);

            EditorGUILayout.HelpBox("All portrait images should use the exact same resolution " +
                "to avoid positioning and tiling issues.", MessageType.Info);

            EditorGUILayout.Separator();

            string[] facingArrows = new string[]
            {
                "FRONT",
                "<--",
                "-->",
            };
            _portraitsFaceProp.enumValueIndex = EditorGUILayout.Popup("Portraits Face", 
                _portraitsFaceProp.enumValueIndex, facingArrows);

            EditorGUILayout.Separator();

            if(EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(chara);

            serializedObject.ApplyModifiedProperties();
        }

    }
}