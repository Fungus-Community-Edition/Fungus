using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Handles population, binding, and persistence of MainAppliers ListView.
    /// Decouples ListView logic from the EditorWindow orchestration.
    /// </summary>
    public sealed class SaveSysMainCodecsController
    {
        private ListView _mainCodecsView;
        private SaveSysSettingsTypeCache _typeCache;
        private SaveSystemSettings _sysSettings;

        /// <summary>
        /// Initializes the controller with references to UI elements and type cache.
        /// </summary>
        public void Init(VisualElement root, SaveSysSettingsTypeCache typeCache)
        {
            _mainCodecsView = root.Q<ListView>("MainCodecs");
            _typeCache = typeCache;
        }

        public void BindToSettings(SaveSystemSettings sysSettings)
        {
            _sysSettings = sysSettings;
            if (_mainCodecsView != null)
            {
                _mainCodecsView.itemsSource = (IList)sysSettings.MainCodecs;
            }
        }

        public void ToggleSubs(bool on)
        {
            if (_mainCodecsView == null)
            {
                return;
            }

            if (on)
            {
                _mainCodecsView.makeItem += OnMakeItem;
                _mainCodecsView.bindItem += OnBindItem;
                _mainCodecsView.unbindItem += OnUnbindItem;
                _mainCodecsView.destroyItem += OnDestroyItem;
                _mainCodecsView.canStartDrag += OnCanStartDrag;
            }
            else
            {
                _mainCodecsView.makeItem -= OnMakeItem;
                _mainCodecsView.bindItem -= OnBindItem;
                _mainCodecsView.unbindItem -= OnUnbindItem;
                _mainCodecsView.destroyItem -= OnDestroyItem;
                _mainCodecsView.canStartDrag -= OnCanStartDrag;
            }
        }

        private VisualElement OnMakeItem()
        {
            var dropdown = new DropdownField
            {
                style =
                {
                    flexGrow = 1,
                    minWidth = 100,
                    marginTop = 15,
                    marginBottom = 15
                }
            };
            return dropdown;
        }

        private void OnBindItem(VisualElement visElem, int index)
        {
            var dropdown = (DropdownField)visElem;
            dropdown.userData = index;
            dropdown.choices = _typeCache.MainCodecChoices.Keys.ToList();
            dropdown.RegisterValueChangedCallback(OnChoiceChanged);

            if (index < _sysSettings.MainCodecs.Count)
            {
                var codec = _sysSettings.MainCodecs[index];
                string displayName = SaveSysTypeUtils.GetDisplayName(codec.GetType());
                if (dropdown.choices.Contains(displayName))
                {
                    dropdown.SetValueWithoutNotify(displayName);
                }
            }
            else
            {
                dropdown.SetValueWithoutNotify(string.Empty);
            }
        }

        private void OnUnbindItem(VisualElement visElem, int index)
        {
            var dropdown = (DropdownField)visElem;
            dropdown.UnregisterValueChangedCallback(OnChoiceChanged);
            visElem.userData = null;
        }

        private void OnDestroyItem(VisualElement element)
        {
            element.userData = null;
            element.Clear();
        }

        private bool OnCanStartDrag(CanStartDragArgs args) => true;

        private void OnChoiceChanged(ChangeEvent<string> evt)
        {
            var dropdown = (DropdownField)evt.target;
            int index = (int)dropdown.userData;
            string choice = evt.newValue;

            if (!_typeCache.MainCodecChoices.TryGetValue(choice, out var codecInstance))
                return;

            if (index < _sysSettings.MainCodecs.Count)
            {
                _sysSettings.SetMainCodecAtIndex(codecInstance, index);
                Debug.Log($"Set MainCodec at index {index} to {codecInstance.GetType().Name}");
            }
            else
            {
                _sysSettings.AddMainCodec(codecInstance);
                Debug.Log($"Added MainCodec {codecInstance.GetType().Name} at index {index}");
            }

            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }
    }
}