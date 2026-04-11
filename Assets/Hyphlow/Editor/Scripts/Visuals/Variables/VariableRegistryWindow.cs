using UnityEditor;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Editor window for viewing and managing the variable registry configuration, including global variable sources.
    /// </summary>
    public sealed class VariableRegistryWindow : EditorWindow
    {
        private SerializedObject _serializedConfig;
        private SerializedProperty _globalSourcesProperty;

        [MenuItem("Window/Atelier Mycelia/Amanita/Variable Registry")]
        public static void BringUp()
        {
            VariableRegistryWindow wnd = GetWindow<VariableRegistryWindow>();
            wnd.titleContent = new GUIContent("Variable Registry");
            wnd.minSize = MinSize;
            wnd.Show();
        }

        private static readonly Vector2 MinSize = new Vector2(320f, 240f);

        private void OnEnable()
        {
            VariableRegistryConfig config = DefaultAssetMaintenance.EnsureVariableRegistryConfig();
            if (config == null)
            {
                return;
            }

            _serializedConfig = new SerializedObject(config);
            _globalSourcesProperty = _serializedConfig.FindProperty("globalSources");
        }

        private void OnGUI()
        {
            if (_serializedConfig == null || _globalSourcesProperty == null)
            {
                EditorGUILayout.HelpBox("VariableRegistryConfig asset not found.", MessageType.Error);
                if (GUILayout.Button("Create Config"))
                {
                    OnEnable();
                }
                return;
            }

            _serializedConfig.Update();

            EditorGUILayout.PropertyField(_globalSourcesProperty, new GUIContent("Global Sources"), true);

            _serializedConfig.ApplyModifiedProperties();

            if (GUILayout.Button("Rebuild Registry"))
            {
                VariableRegistryService.EnsureDefault().Rebuild();
            }
        }
    }
}