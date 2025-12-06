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
    public sealed class SaveSysMainAppliersController
    {
        private ListView _mainAppliersView;
        private SaveSysSettingsTypeCache _typeCache;
        private SaveSystemSettings _sysSettings;

        /// <summary>
        /// Initializes the controller with references to UI elements and type cache.
        /// </summary>
        public void Init(VisualElement root, SaveSysSettingsTypeCache typeCache)
        {
            _mainAppliersView = root.Q<ListView>("MainAppliers");
            _typeCache = typeCache;
        }

        public void BindToSettings(SaveSystemSettings sysSettings)
        {
            _sysSettings = sysSettings;
            if (_mainAppliersView != null)
            {
                _mainAppliersView.itemsSource = (IList)sysSettings.MainAppliers;
            }
        }

        public void ToggleSubs(bool on)
        {
            if (_mainAppliersView == null)
            {
                return;
            }

            if (on)
            {
                _mainAppliersView.makeItem += OnMakeItem;
                _mainAppliersView.bindItem += OnBindItem;
                _mainAppliersView.unbindItem += OnUnbindItem;
                _mainAppliersView.destroyItem += OnDestroyItem;
                _mainAppliersView.canStartDrag += OnCanStartDrag;
            }
            else
            {
                _mainAppliersView.makeItem -= OnMakeItem;
                _mainAppliersView.bindItem -= OnBindItem;
                _mainAppliersView.unbindItem -= OnUnbindItem;
                _mainAppliersView.destroyItem -= OnDestroyItem;
                _mainAppliersView.canStartDrag -= OnCanStartDrag;
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
            dropdown.choices = _typeCache.MainApplierChoices.Keys.ToList();
            dropdown.RegisterValueChangedCallback(OnChoiceChanged);

            if (index < _sysSettings.MainAppliers.Count)
            {
                var applier = _sysSettings.MainAppliers[index];
                string displayName = SaveSysTypeUtils.GetDisplayName(applier.GetType());
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

            if (!_typeCache.MainApplierChoices.TryGetValue(choice, out var applierInstance))
                return;

            if (index < _sysSettings.MainAppliers.Count)
            {
                _sysSettings.SetMainApplierAtIndex(applierInstance, index);
                Debug.Log($"Set MainApplier at index {index} to {applierInstance.GetType().Name}");
            }
            else
            {
                _sysSettings.AddMainApplier(applierInstance);
                Debug.Log($"Added MainApplier {applierInstance.GetType().Name} at index {index}");
            }

            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }
    }
}