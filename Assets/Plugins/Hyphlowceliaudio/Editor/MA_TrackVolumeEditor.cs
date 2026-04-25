#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;

namespace AtMycelia.Hyphlowceliaudio
{
    [CustomEditor(typeof(MA_TrackVolume), true)]
    public class MA_TrackVolumeEditor : CommandEditor
    {
        public override void OnEnable()
        {
            base.OnEnable();

            _trackGroupProperty = serializedObject.FindProperty("_trackGroup");
            _operationProperty = serializedObject.FindProperty("_operation");
            _trackProperty = serializedObject.FindProperty("_track");
            _targetVolProperty = serializedObject.FindProperty("_targetVol");
            _trackSelectionProperty = serializedObject.FindProperty("_trackSelection");
            _outputVarProperty = serializedObject.FindProperty("_outputVar");
        }

        private SerializedProperty _trackGroupProperty;
        private SerializedProperty _operationProperty;
        private SerializedProperty _trackProperty;
        private SerializedProperty _targetVolProperty;
        private SerializedProperty _trackSelectionProperty;
        private SerializedProperty _outputVarProperty;

        public override void DrawCommandGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_trackGroupProperty);
            EditorGUILayout.PropertyField(_operationProperty);
            EditorGUILayout.PropertyField(_trackProperty);
            DrawTargetVol();

            bool trackSelectionChanged = DrawTrackSelection();
            if (trackSelectionChanged)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(_outputVarProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private bool DrawTrackSelection()
        {
            if (_trackSelectionProperty == null)
            {
                return false;
            }

            TrackSelection selection = (TrackSelection)_trackSelectionProperty.enumValueIndex;

            EditorGUI.BeginChangeCheck();
            selection = (TrackSelection)EditorGUILayout.EnumPopup(new GUIContent("Track Selection"), selection);
            if (EditorGUI.EndChangeCheck())
            {
                _trackSelectionProperty.enumValueIndex = (int)selection;
                return true;
            }

            return false;
        }
    
        private void DrawTargetVol()
        {
            // We only want to do so when we're setting the volume, not when
            // we're getting it. When we're getting it, the target vol field
            // is irrelevant and just takes up space.
            GetOrSet operation = (GetOrSet)_operationProperty.enumValueIndex;
            if (operation == GetOrSet.Set)
            {
                EditorGUILayout.PropertyField(_targetVolProperty);
            }
        }
    }
}
#endif