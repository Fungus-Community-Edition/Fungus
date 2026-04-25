using AtMycelia.Hyphlow.Sys;
using AtMycelia.AmaniTween;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public sealed class HyphlowRuntimeSysResourcesWindow : EditorWindow
    {
        private const string ResourcesSubfolderPath = "AtMycelia/Hyphlow/Sys";
        private const string AssetName = "HyphlowRuntimeSysAssets";

        [MenuItem("Window/Atelier Mycelia/Hyphlow/Sys/Hyphlow Runtime Sys Resources")]
        public static void Open()
        {
            HyphlowRuntimeSysResourcesWindow window = GetWindow<HyphlowRuntimeSysResourcesWindow>();
            window.titleContent = new GUIContent("Hyphlow Runtime Sys Resources");
            window.Show();
        }

        private void OnEnable()
        {
            if (_s != null && _s != this)
            {
                _s.Focus();
                Close();
                return;
            }

            _s = this;
            EnsureResources();
        }

        private static HyphlowRuntimeSysResourcesWindow _s;

        private void EnsureResources()
        {
            if (_assets != null)
            {
                return;
            }

            _assets = SOUtils.EnsureSOExists<HyphlowRuntimeSysAssets>(ResourcesSubfolderPath, AssetName);
        }

        private HyphlowRuntimeSysAssets _assets;

        private void OnDisable()
        {
            if (_s == this)
            {
                _s = null;
            }
        }

        private void OnFocus()
        {
            RefreshFields();
        }

        public void CreateGUI()
        {
            EnsureResources();

            rootVisualElement.Clear();

            if (_assets == null)
            {
                rootVisualElement.Add(new HelpBox("HyphlowRuntimeSysAssets asset not found.", HelpBoxMessageType.Error));
                return;
            }

            VisualElement content = new VisualElement();
            content.style.paddingLeft = 8f;
            content.style.paddingRight = 8f;
            content.style.paddingTop = 8f;
            content.style.paddingBottom = 8f;

            _tweenAdapterField = new ObjectField("Tween Adapter")
            {
                objectType = typeof(DefaultTweenAdapter),
                allowSceneObjects = false
            };
            _tweenAdapterField.RegisterValueChangedCallback(OnTweenAdapterChanged);

            _variableRegistryConfigField = new ObjectField("Variable Registry Config")
            {
                objectType = typeof(VariableRegistryConfig),
                allowSceneObjects = false
            };
            _variableRegistryConfigField.RegisterValueChangedCallback(OnVariableRegistryConfigChanged);

            content.Add(_tweenAdapterField);
            content.Add(_variableRegistryConfigField);

            rootVisualElement.Add(content);

            RefreshFields();
        }

        private ObjectField _tweenAdapterField;
        private ObjectField _variableRegistryConfigField;

        private void RefreshFields()
        {
            if (_assets == null)
            {
                return;
            }

            if (_tweenAdapterField != null)
            {
                _tweenAdapterField.SetValueWithoutNotify(_assets.TweenAdapter);
            }

            if (_variableRegistryConfigField != null)
            {
                _variableRegistryConfigField.SetValueWithoutNotify(_assets.VariableRegistryConfig);
            }
        }

        private void OnTweenAdapterChanged(ChangeEvent<Object> evt)
        {
            if (_assets == null)
            {
                return;
            }

            _assets.TweenAdapter = evt.newValue as DefaultTweenAdapter;
            _tweenAdapterField.SetValueWithoutNotify(_assets.TweenAdapter);
        }

        private void OnVariableRegistryConfigChanged(ChangeEvent<Object> evt)
        {
            if (_assets == null)
            {
                return;
            }

            _assets.VariableRegistryConfig = evt.newValue as VariableRegistryConfig;
            _variableRegistryConfigField.SetValueWithoutNotify(_assets.VariableRegistryConfig);
        }
    }
}