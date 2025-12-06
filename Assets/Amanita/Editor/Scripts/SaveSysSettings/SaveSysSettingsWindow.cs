using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using Type = System.Type;
using UnityEditor.UIElements;
using System.Linq;

namespace Amanita.SaveSys.EditorUtils
{
    public sealed class SaveSysSettingsWindow : EditorWindow
    {
        // Enforced single instance
        public static SaveSysSettingsWindow Instance { get; set; }

        // Persist prior UI selections across recreation
        private static string _lastReaderChoice;
        private static string _lastWriterChoice;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Atelier Mycelia/Amanita/Save Sys Settings")]
        public static void Open()
        {
            if (Instance != null)
            {
                Instance.ApplySizeConstraints(); // In case they were lost.
                Instance.Focus();
                // No more need for setup (remember, we want only one instance active at a time), so...
                return;
            }

            // GetWindow will reuse an existing one of the same type if present.
            SaveSysSettingsWindow wnd = GetWindow<SaveSysSettingsWindow>();
            wnd.titleContent = new GUIContent("Save Sys Settings");
            wnd.ApplySizeConstraints();

            Instance = wnd;
            wnd.EnsureSingleInstance();

            var settings = GetSysSettings();
            static SaveSystemSettings GetSysSettings()
            {
                SaveSystemSettings sysSettings = Resources.Load<SaveSystemSettings>("SaveSys/Settings/SaveSystemSettings");
                if (sysSettings == null)
                {
                    sysSettings = SOUtils.GetOrCreateScriptableObject<SaveSystemSettings>("SaveSys/Settings",
                        "SaveSystemSettings");

                    if (sysSettings != null)
                    {
                        Debug.Log("Created SaveSystemSettings asset in Resources/SaveSys/Settings folder.");
                    }
                    else
                    {
                        Debug.LogError("Failed to create SaveSystemSettings asset.");
                    }
                }
                return sysSettings;
            }

            wnd.SysSettings = settings; // Runs after CreateGUI via property setter
            wnd.Refresh();
            wnd.Focus();
        }

        private void ApplySizeConstraints()
        {
            minSize = maxSize = windowSize;
        }

        private static Vector2 windowSize = new Vector2(600, 700);

        private void EnsureSingleInstance()
        {
            if (Instance == null)
            {
                Instance = this;
                return;
            }

            if (Instance != this)
            {
                // A second window appeared; close this duplicate.
                Close();
            }
        }

        private void OnEnable()
        {
            EnsureSingleInstance();
            // Apply constraints when enabling (covers domain reload).
            if (Instance == this)
            {
                minSize = maxSize = windowSize;
                // Avoid forcing size larger than current if user kept it >= constraints.
                var currentSize = position.size;
                if (currentSize.x < windowSize.x || currentSize.y < windowSize.y)
                {
                    position = new Rect(position.position, windowSize);
                }
            }

            if (_rootIsReady)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                // Record current choices before clearing instance.
                RecordCurrentChoices();
                Instance = null;
            }
        }

        /// <summary>
        /// Executes every time the window opens, prepping its gui for the world to see.
        /// </summary>
        public void CreateGUI()
        {
            VisualElement mainUxml = m_VisualTreeAsset.Instantiate();
            Root.Add(mainUxml);
            _rootIsReady = true;

            UpdateCacheFromTypeRegistries();
            RegisterViews();
            RefreshDropdowns();

            _dropdownController.PrepReaderAndWriterInstances();
            
            RestoreLastChoicesIfAny();
            ToggleSubs(true);
            FillMissingAssetSettingsBasedOnUi();
        }


        private bool _rootIsReady = false;
        private VisualElement Root => rootVisualElement;

        private SaveSysDropdownController _dropdownController = new SaveSysDropdownController();

        private void RegisterViews()
        {
            if (!_rootIsReady)
            {
                // This can happen when Refresh is called before CreateGUI.
                return;
            }

            _dropdownController.Init(Root, _typeCache);

            storageSettings = Root.Q<ObjectField>("StorageSettings");
            _refreshButton = Root.Q<Button>("RefreshButton");
            _mainAppliersView = Root.Q<ListView>("MainAppliers");
        }

        private DropdownField SaveReaderDropdown
        {
            get
            {
                if (_dropdownController == null)
                {
                    return null;
                }

                return _dropdownController.ReaderDropdown;
            }
        }

        private DropdownField SaveWriterDropdown
        {
            get
            {
                if (_dropdownController == null)
                {
                    return null;
                }
                return _dropdownController.WriterDropdown;
            }
        }

        private ObjectField storageSettings;
        private Button _refreshButton;
        private ListView _mainAppliersView;
        // ^Separate from the version in SysSettings to avoid direct coupling in the UI.

        private static readonly SaveSysSettingsTypeCache _typeCache = new SaveSysSettingsTypeCache();

        private void UpdateCacheFromTypeRegistries()
        {
            _typeCache.Refresh();
        }

        private void RefreshDropdowns()
        {
            if (_dropdownController == null)
            {
                return;
            }

            _dropdownController.Refresh();
        }

        private static string GetDisplayName(Type type)
        {
            return SaveSysTypeUtils.GetDisplayName(type);
        }

        private void RestoreLastChoicesIfAny()
        {
            if (SaveReaderDropdown != null &&
                !string.IsNullOrEmpty(_lastReaderChoice) &&
                SaveReaderDropdown.choices.Contains(_lastReaderChoice))
            {
                SaveReaderDropdown.SetValueWithoutNotify(_lastReaderChoice);
            }

            if (SaveWriterDropdown != null &&
                !string.IsNullOrEmpty(_lastWriterChoice) &&
                SaveWriterDropdown.choices.Contains(_lastWriterChoice))
            {
                SaveWriterDropdown.SetValueWithoutNotify(_lastWriterChoice);
            }

        }

        #region Event Subscriptions

        private void ToggleSubs(bool on)
        {
            if (!_rootIsReady)
            {
                return;
            }

            if (on)
            {
                storageSettings.RegisterValueChangedCallback(OnStorageSettingsChanged);
                SaveReaderDropdown.RegisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                SaveWriterDropdown.RegisterValueChangedCallback(OnWriterDropdownChoiceChanged);
                _refreshButton.clicked += Refresh;
            }
            else
            {
                storageSettings.UnregisterValueChangedCallback(OnStorageSettingsChanged);
                SaveReaderDropdown.UnregisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                SaveWriterDropdown.UnregisterValueChangedCallback(OnWriterDropdownChoiceChanged);
                _refreshButton.clicked -= Refresh;
            }

            ToggleForMainAppliers(on);
        }

        private void OnStorageSettingsChanged(ChangeEvent<Object> evt)
        {
            _sysSettings.StorageSettings = evt.newValue as SaveStorageSettings;
            MakeSysSettingsChangesStick();
        }

        private void OnReaderDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            _dropdownController.AssignSelection(evt.newValue, true, so =>
            {
                _sysSettings.SaveReader = so as ISaveReader;
                if (so != null)
                {
                    _lastReaderChoice = SaveReaderDropdown.value;
                    MakeSysSettingsChangesStick();
                }
            });
        }

        private void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            _dropdownController.AssignSelection(evt.newValue, false, so =>
            {
                _sysSettings.SaveWriter = so as ISaveWriter;
                if (so != null)
                {
                    _lastWriterChoice = SaveWriterDropdown.value;
                    MakeSysSettingsChangesStick();
                }
            });
        }

        private void ToggleForMainAppliers(bool on)
        {
            if (on)
            {
                _mainAppliersView.makeItem += OnMakeItemForMainAppliers;
                _mainAppliersView.bindItem += OnBindItemForMainAppliers;
                _mainAppliersView.unbindItem += OnUnbindItemForMainAppliers;
                _mainAppliersView.destroyItem += OnDestroyItemForMainAppliers;
                _mainAppliersView.canStartDrag += OnCanStartDragForMainAppliers;
            }
            else
            {
                _mainAppliersView.makeItem -= OnMakeItemForMainAppliers;
                _mainAppliersView.bindItem -= OnBindItemForMainAppliers;
                _mainAppliersView.unbindItem -= OnUnbindItemForMainAppliers;
                _mainAppliersView.destroyItem -= OnDestroyItemForMainAppliers;
                _mainAppliersView.canStartDrag -= OnCanStartDragForMainAppliers;
            }
        }

        private VisualElement OnMakeItemForMainAppliers()
        {
            var dropdown = new DropdownField();
            dropdown.style.flexGrow = 1;
            dropdown.style.minWidth = 100;
            dropdown.style.marginBottom = dropdown.style.marginTop = 15;
            return dropdown;
        }

        private void OnMainApplierDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            DropdownField dropdown = (DropdownField)evt.target;
            // Find the applier tied to the current choice
            string currentChoice = evt.newValue;
            ISaveDataApplier applierInstance = _typeCache.MainApplierChoices[currentChoice];

            int index = (int)dropdown.userData;
            bool changeAtIndex = index < SysSettings.MainAppliers.Count;
            if (changeAtIndex)
            {
                SysSettings.SetMainApplierAtIndex(applierInstance, index);
                Debug.Log($"Set MainApplier at index {index} to {applierInstance.GetType().Name}");
            }
            else
            {
                SysSettings.AddMainApplier(applierInstance);
                Debug.Log($"Added MainApplier {applierInstance.GetType().Name} at index {index}");
            }

            MakeSysSettingsChangesStick();

        }

        private void OnBindItemForMainAppliers(VisualElement visElem, int index)
        {
            DropdownField dropdown = (DropdownField)visElem;
            dropdown.userData = index;
            dropdown.choices = _typeCache.MainApplierChoices.Keys.ToList();
            dropdown.RegisterValueChangedCallback(OnMainApplierDropdownChoiceChanged);
            // Set initial value based on current settings
            if (index < SysSettings.MainAppliers.Count)
            {
                var applier = SysSettings.MainAppliers[index];
                string displayName = GetDisplayName(applier.GetType());
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

        private void OnUnbindItemForMainAppliers(VisualElement visElem, int index)
        {
            DropdownField dropdown = (DropdownField)visElem;
            dropdown.UnregisterValueChangedCallback(OnMainApplierDropdownChoiceChanged);
            visElem.userData = null;
        }

        private void OnDestroyItemForMainAppliers(VisualElement element)
        {
            element.userData = null; // In case we decide to assign such in the future
            element.Clear();
        }

        private bool OnCanStartDragForMainAppliers(CanStartDragArgs args)
        {
            return true;
        }

        #endregion

        #region SaveSystemSettings Synchronization

        private void MakeSysSettingsChangesStick()
        {
            if (_sysSettings == null)
            {
                return;
            }
            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        private void FillMissingAssetSettingsBasedOnUi()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null. Cannot fill missing asset settings.");
                return;
            }

            FillForReaderAndWriter();
            void FillForReaderAndWriter()
            {
                if (_sysSettings.SaveReader == null && !string.IsNullOrEmpty(SaveReaderDropdown?.value))
                {
                    var readerInstance = _dropdownController.GetInstanceForChoice(SaveReaderDropdown.value, isReader: true);
                    _sysSettings.SaveReader = readerInstance as ISaveReader;

                }

                if (_sysSettings.SaveWriter == null && !string.IsNullOrEmpty(SaveWriterDropdown?.value))
                {
                    var writerInstance = _dropdownController.GetInstanceForChoice(SaveWriterDropdown.value, isReader: false);
                    _sysSettings.SaveWriter = writerInstance as ISaveWriter;
                }
            }

            RecordCurrentChoices();
        }

        private void RecordCurrentChoices()
        {
            if (SaveReaderDropdown != null && !string.IsNullOrEmpty(SaveReaderDropdown.value))
            {
                _lastReaderChoice = SaveReaderDropdown.value;
            }

            if (SaveWriterDropdown != null && !string.IsNullOrEmpty(SaveWriterDropdown.value))
            {
                _lastWriterChoice = SaveWriterDropdown.value;
            }
        }

        private void Refresh()
        {
            UpdateCacheFromTypeRegistries();

            if (!_rootIsReady) // For when called before CreateGUI
            {
                return;
            }

            RegisterViews();
            RefreshDropdowns();

            // Directly use the controller here too
            _dropdownController?.PrepReaderAndWriterInstances();

            ToggleSubs(false);
            ToggleSubs(true);
            ApplySettingsAssetToUI();
        }

        private void ApplySettingsAssetToUI()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null, cannot apply to UI.");
                return;
            }

            storageSettings?.SetValueWithoutNotify(_sysSettings.StorageSettings);

            if (SysSettings.SaveReader != null)
            {
                var readerType = SysSettings.SaveReader.GetType();
                string choice = GetDisplayName(readerType);
                if (SaveReaderDropdown.choices.Contains(choice))
                {
                    SaveReaderDropdown.SetValueWithoutNotify(choice);
                    _lastReaderChoice = choice;
                }
            }

            if (SysSettings.SaveWriter != null)
            {
                var writerType = SysSettings.SaveWriter.GetType();
                string choice = GetDisplayName(writerType);
                if (SaveWriterDropdown.choices.Contains(choice))
                {
                    SaveWriterDropdown.SetValueWithoutNotify(choice);
                    _lastWriterChoice = choice;
                }
            }

            _mainAppliersView.itemsSource = (IList)SysSettings.MainAppliers;
        }

        private SaveSystemSettings SysSettings
        {
            get => _sysSettings;
            set
            {
                if (_sysSettings != value)
                {
                    _sysSettings = value;
                    Refresh();
                }
            }
        }
        private SaveSystemSettings _sysSettings;

        private void OnDisable()
        {
            ToggleSubs(false);
            _rootIsReady = false;
        }
    
        #endregion

        public class TypeChoiceInfo
        {
            public Type Type { get; set; }
            public string ChoiceText { get; set; }
        }

    }
}