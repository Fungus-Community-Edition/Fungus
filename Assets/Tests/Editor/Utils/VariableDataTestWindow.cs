using UnityEditor;
using UnityEngine;

namespace VScriptingTests.VariableOperations
{
    // Testing-only window to exercise property drawers via Unity's IMGUI pipeline
    public class VariableDataTestWindow : EditorWindow
    {
        private SerializedObject _so;
        private SerializedProperty _dataProp;
        private Object _target;

        public static VariableDataTestWindow Show(Object target, string propertyPath = "data")
        {
            var wnd = GetWindow<VariableDataTestWindow>("VariableData Test");
            wnd.SetTarget(target, propertyPath);
            wnd.Show();
            return wnd;
        }

        public void SetTarget(Object target, string propertyPath)
        {
            _target = target;
            _so = new SerializedObject(target);
            _dataProp = _so.FindProperty(propertyPath);
        }

        private void OnGUI()
        {
            if (_so == null || _dataProp == null)
            {
                EditorGUILayout.HelpBox("No target or property bound.", MessageType.Info);
                return;
            }

            _so.Update();

            // Draw organically; Unity resolves VariableDataDrawer
            EditorGUILayout.PropertyField(_dataProp, new GUIContent("VariableData"), includeChildren: true);

            if (_so.hasModifiedProperties)
            {
                _so.ApplyModifiedProperties();
            }
        }
    }
}