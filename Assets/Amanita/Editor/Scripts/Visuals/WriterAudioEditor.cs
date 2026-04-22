using AtMycelia.Amanita;
using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [CustomEditor (typeof(WriterAudio))]
    public class WriterAudioEditor : Editor
    {
        protected SerializedProperty _volumeProp;
        protected SerializedProperty _loopProp;
        protected SerializedProperty _targetAudioSourceProp;
        protected SerializedProperty _audioModeProp;
        protected SerializedProperty _beepSoundsProp;
        protected SerializedProperty _soundEffectProp;
        protected SerializedProperty _inputSoundProp;
        protected SerializedProperty _useLegacyAudioLogicProp;

        protected virtual void OnEnable()
        {
            //volumeProp = serializedObject.FindProperty("volume");
            _loopProp = serializedObject.FindProperty("_loop");
            _targetAudioSourceProp = serializedObject.FindProperty("_targetAudioSource");
            _inputSoundProp = serializedObject.FindProperty("_inputSound");
            _audioModeProp = serializedObject.FindProperty("_audioMode");
            _beepSoundsProp = serializedObject.FindProperty("_beepSounds");
            _soundEffectProp = serializedObject.FindProperty("_soundEffect");
            _useLegacyAudioLogicProp = serializedObject.FindProperty("_useLegacyAudioLogic");
        }

        public override void OnInspectorGUI() 
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_loopProp);
            EditorGUILayout.PropertyField(_inputSoundProp);
            EditorGUILayout.PropertyField(_audioModeProp);
            if ((AudioMode)_audioModeProp.enumValueIndex == AudioMode.Beeps)
            {
                EditorGUILayout.PropertyField(_beepSoundsProp, 
                    new GUIContent("Beep Sounds", "A list of beep sounds to play at random"),true);
            }
            else
            {
                EditorGUILayout.PropertyField(_soundEffectProp);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }    
}