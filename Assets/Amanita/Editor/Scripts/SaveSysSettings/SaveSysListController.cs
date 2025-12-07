using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;

namespace Amanita.SaveSys.EditorUtils
{
    public sealed class SaveSysListController<T>
    {
        private readonly string _listViewName;
        private readonly Func<SaveSysSettingsTypeCache, IReadOnlyDictionary<string, T>> _choicesSelector;
        private readonly Func<SaveSystemSettings, IList> _collectionSelector;
        private readonly Action<SaveSystemSettings, T, int> _setAtIndex;
        private readonly Action<SaveSystemSettings, T> _addItem;
        private readonly Action<SaveSystemSettings, T> _removeItem;

        private ListView _listView;
        private SaveSysSettingsTypeCache _typeCache;
        private SaveSystemSettings _sysSettings;

        /// <summary>
        /// ChoicesSelector: Given the type cache, returns a dictionary mapping display names to instances of T.
        /// CollectionSelector: Given the SaveSystemSettings, returns the IList collection of T instances.
        /// SetAtIndex: Given the SaveSystemSettings, an instance of T, and an index, sets the item at that index.
        /// AddItem: Given the SaveSystemSettings and an instance of T, adds it to the collection.
        /// </summary>
        public SaveSysListController(
            string listViewName,
            Func<SaveSysSettingsTypeCache, IReadOnlyDictionary<string, T>> choicesSelector,
            Func<SaveSystemSettings, IList> collectionSelector,
            Action<SaveSystemSettings, T, int> setAtIndex,
            Action<SaveSystemSettings, T> addItem,
            Action<SaveSystemSettings, T> removeItem)
        {
            _listViewName = listViewName;
            _choicesSelector = choicesSelector;
            _collectionSelector = collectionSelector;
            _setAtIndex = setAtIndex;
            _addItem = addItem;
            _removeItem = removeItem;
        }

        public void Init(VisualElement root, SaveSysSettingsTypeCache typeCache)
        {
            _listView = root.Q<ListView>(_listViewName);
            _typeCache = typeCache;
        }

        public void BindToSettings(SaveSystemSettings sysSettings)
        {
            _sysSettings = sysSettings;
            if (_listView != null)
            {
                _listView.itemsSource = _collectionSelector(sysSettings);
            }
        }

        public void ToggleSubs(bool on)
        {
            if (_listView == null)
            {
                return;
            }

            if (on)
            {
                _listView.makeItem += OnMakeItem;
                _listView.bindItem += OnBindItem;
                _listView.unbindItem += OnUnbindItem;
                _listView.destroyItem += OnDestroyItem;
                _listView.canStartDrag += OnCanStartDrag;
                _listView.itemsRemoved += OnItemRemoved;
            }
            else
            {
                _listView.makeItem -= OnMakeItem;
                _listView.bindItem -= OnBindItem;
                _listView.unbindItem -= OnUnbindItem;
                _listView.destroyItem -= OnDestroyItem;
                _listView.canStartDrag -= OnCanStartDrag;
                _listView.itemsRemoved -= OnItemRemoved;
            }
        }

        

        private VisualElement OnMakeItem()
        {
            return new DropdownField
            {
                style =
                {
                    flexGrow = 1,
                    minWidth = 100,
                    height = _listView.fixedItemHeight * 0.9f
                }
            };
        }

        private void OnBindItem(VisualElement visElem, int index)
        {
            var dropdown = (DropdownField)visElem;
            dropdown.userData = index;

            var choices = _choicesSelector(_typeCache).Keys.ToList();
            dropdown.choices = choices;
            dropdown.RegisterValueChangedCallback(OnChoiceChanged);

            var collection = _collectionSelector(_sysSettings);
            if (index < collection.Count)
            {
                var item = collection[index];
                string displayName = SaveSysTypeUtils.GetDisplayName(item.GetType());
                if (choices.Contains(displayName))
                    dropdown.SetValueWithoutNotify(displayName);
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

            var dict = _choicesSelector(_typeCache);
            if (!dict.TryGetValue(choice, out var instance)) return;

            var collection = _collectionSelector(_sysSettings);
            if (index < collection.Count)
            {
                _setAtIndex(_sysSettings, instance, index);
            }
            else
            {
                _addItem(_sysSettings, instance);
            }

            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        private void OnItemRemoved(IEnumerable<int> indexesOfWhatWasRemoved)
        {
            // We assume that only one thing can be removed at a time
            int index = indexesOfWhatWasRemoved.First();
            T whatToRemove = (T)_collectionSelector(_sysSettings)[index];
            _removeItem(_sysSettings, whatToRemove);

        }
    }
}