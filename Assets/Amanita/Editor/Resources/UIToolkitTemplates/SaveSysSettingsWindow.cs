using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Type = System.Type;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;
using Collections;

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
            // If already open, just focus and return.
            if (Instance != null)
            {
                // Re-apply size constraints in case they were lost.
                Instance.minSize = Instance.maxSize = windowSize;
                // Only enforce position size if user previously resized beyond constraints.
                var currentSize = Instance.position.size;
                if (currentSize.x < windowSize.x || currentSize.y < windowSize.y)
                {
                    Instance.position = new Rect(Instance.position.position, windowSize);
                }
                Instance.Focus();
                return;
            }

            // GetWindow will reuse an existing one of the same type if present.
            SaveSysSettingsWindow wnd = GetWindow<SaveSysSettingsWindow>();
            wnd.titleContent = new GUIContent("Save Sys Settings");
            wnd.minSize = wnd.maxSize = windowSize;

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
            Refresh();
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

            RegisterViews();
            UpdateCacheFromTypeRegistries();
            RefreshDropdowns();
            PrepReaderAndWriterInstances();
            RestoreLastChoicesIfAny();
            ToggleSubs(true);
            FillMissingAssetSettingsBasedOnUi();
        }


        private bool _rootIsReady = false;
        private VisualElement Root => rootVisualElement;

        private void RegisterViews()
        {
            if (!_rootIsReady)
            {
                // This can happen when Refresh is called before CreateGUI.
                return;
            }
            saveReaderDropdown = Root.Q<DropdownField>("SaveReaderDropdown");
            saveWriterDropdown = Root.Q<DropdownField>("SaveWriterDropdown");
            storageSettings = Root.Q<ObjectField>("StorageSettings");
            _refreshButton = Root.Q<Button>("RefreshButton");
            _mainAppliersView = Root.Q<ListView>("MainAppliers");
        }

        private DropdownField saveReaderDropdown, saveWriterDropdown;
        private ObjectField storageSettings;
        private Button _refreshButton;
        private ListView _mainAppliersView;
        private readonly List<ISaveDataApplier> _mainAppliersForUi = new List<ISaveDataApplier>();
        // ^Separate from the version in SysSettings to avoid direct coupling in the UI.
        private readonly List<DropdownField> _mainApplierDropdowns = new List<DropdownField>();
        
        private void UpdateCacheFromTypeRegistries()
        {
            ClearCaches();
            void ClearCaches()
            {
                validReaderTypes.Clear();
                validWriterTypes.Clear();
                validMainApplierTypes.Clear();
                validMainApplierChoices.Clear();
            }
            
            PopulateReaderAndWriterChoices();
            void PopulateReaderAndWriterChoices()
            {
                IList<Type> scriptableObjReaders = SaveReaderTypeRegistry.Types
                    .Where(typeEl => !typeEl.Name.Contains("Test") &&
                                     scriptableObjType.IsAssignableFrom(typeEl) &&
                                     iSaveReaderType.IsAssignableFrom(typeEl)).ToList();
                validReaderTypes.AddRange(scriptableObjReaders);

                IList<Type> scriptableObjWriters = SaveWriterTypeRegistry.Types
                    .Where(typeEl => !typeEl.Name.Contains("Test") &&
                                     scriptableObjType.IsAssignableFrom(typeEl) &&
                                     iSaveWriterType.IsAssignableFrom(typeEl)).ToList();
                validWriterTypes.AddRange(scriptableObjWriters);
            }

            PopulateMainApplierTypesAndChoices();
            void PopulateMainApplierTypesAndChoices()
            {
                IList<Type> mainApplierTypes = SaveDataApplierRegistry.Types
                .Where(typeEl => !typeEl.Name.Contains("Test") &&
                                 scriptableObjType.IsAssignableFrom(typeEl) &&
                                 iSaveDataApplierType.IsAssignableFrom(typeEl)).ToList();
                validMainApplierTypes.AddRange(mainApplierTypes);

                for (int i = 0; i < validMainApplierTypes.Count; i++)
                {
                    var applierType = validMainApplierTypes[i];
                    string displayName = GetDisplayName(applierType);
                    string displayNameWithUnderscores = displayName.Replace(space, underscore);
                    string assetName = $"Generated_{displayNameWithUnderscores}";
                    var applierInstance = SOUtils.GetOrCreateScriptableObject(
                        applierType,
                        "SaveSys/SaveAppliers",
                        assetName);
                    if (applierInstance is ISaveDataApplier applier)
                    {
                        validMainApplierChoices[displayName] = applier;
                    }
                }
            }
        }

        private readonly static IList<Type> validReaderTypes = new List<Type>();
        private readonly static IList<Type> validWriterTypes = new List<Type>();
        private readonly static IList<Type> validMainApplierTypes = new List<Type>();

        // Keys are the display names, values are the applier instances
        private readonly static Dictionary<string, ISaveDataApplier> validMainApplierChoices = 
            new Dictionary<string, ISaveDataApplier>();

        private readonly static Type scriptableObjType = typeof(ScriptableObject);
        private readonly static Type iSaveReaderType = typeof(ISaveReader);
        private readonly static Type iSaveWriterType = typeof(ISaveWriter);
        private readonly static Type iSaveDataApplierType = typeof(ISaveDataApplier);

        private void RefreshDropdowns()
        {
            if (!_rootIsReady)
            {
                return;
            }

            readerTypeMap.Clear();
            writerTypeMap.Clear();
            saveReaderDropdown.choices.Clear();
            saveWriterDropdown.choices.Clear();

            Populate(readerTypeMap, validReaderTypes, saveReaderDropdown);
            Populate(writerTypeMap, validWriterTypes, saveWriterDropdown);

            static void Populate(IDictionary<string, Type> typeMap, IList<Type> validTypes, DropdownField dropdown)
            {
                foreach (var type in validTypes)
                {
                    string name = GetDisplayName(type);
                    if (name.Contains("Test"))
                    {
                        continue;
                    }
                    typeMap[name] = type;
                    dropdown.choices.Add(name);
                }
            }
            // Do not assign initial values here.
        }

        private static IDictionary<string, Type> readerTypeMap = new Dictionary<string, Type>();
        private static IDictionary<string, Type> writerTypeMap = new Dictionary<string, Type>();

        static void PrepReaderAndWriterInstances()
        {
            readerInstanceMap.Clear();
            writerInstanceMap.Clear();

            RegisterDefaultsFor(readerInstanceMap, iSaveReaderType);
            RegisterDefaultsFor(writerInstanceMap, iSaveWriterType);

            GetOrGenerateAssetsFor(readerInstanceMap, validReaderTypes);
            GetOrGenerateAssetsFor(writerInstanceMap, validWriterTypes);

            static void RegisterDefaultsFor(IDictionary<TypeChoiceInfo, ScriptableObject> map, Type interfaceType)
            {
                const string defaultsSubfolder = AmanitaConstants.PathToSaveSysDefaultsFolder;
                IList<ScriptableObject> defaultInstances = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                    .Where(elem => elem != null && interfaceType.IsAssignableFrom(elem.GetType()))
                    .ToList();

                foreach (var elem in defaultInstances)
                {
                    Type elemType = elem.GetType();
                    TypeChoiceInfo info = new TypeChoiceInfo
                    {
                        Type = elemType,
                        ChoiceText = GetDisplayName(elemType)
                    };
                    map[info] = elem;
                }
            }

            static void GetOrGenerateAssetsFor(IDictionary<TypeChoiceInfo, ScriptableObject> map, IList<Type> validTypes)
            {
                const string settingsSubfolder = "SaveSys/Settings";
                foreach (var type in validTypes)
                {
                    bool alreadyRegistered = map.Keys.Any(keyEl => keyEl.Type == type);
                    if (alreadyRegistered)
                    {
                        continue;
                    }

                    string assetName = $"Generated{type.FullName}";
                    ScriptableObject instance = SOUtils.GetOrCreateScriptableObject(type, settingsSubfolder, assetName);
                    TypeChoiceInfo choiceInfo = new TypeChoiceInfo
                    {
                        Type = type,
                        ChoiceText = GetDisplayName(type)
                    };
                    map[choiceInfo] = instance;
                }
            }
        }

        private static readonly IDictionary<TypeChoiceInfo, ScriptableObject> readerInstanceMap = 
            new Dictionary<TypeChoiceInfo, ScriptableObject>();
        private static readonly IDictionary<TypeChoiceInfo, ScriptableObject> writerInstanceMap = 
            new Dictionary<TypeChoiceInfo, ScriptableObject>();

        private void RestoreLastChoicesIfAny()
        {
            if (saveReaderDropdown != null &&
                !string.IsNullOrEmpty(_lastReaderChoice) &&
                saveReaderDropdown.choices.Contains(_lastReaderChoice))
            {
                saveReaderDropdown.SetValueWithoutNotify(_lastReaderChoice);
            }

            if (saveWriterDropdown != null &&
                !string.IsNullOrEmpty(_lastWriterChoice) &&
                saveWriterDropdown.choices.Contains(_lastWriterChoice))
            {
                saveWriterDropdown.SetValueWithoutNotify(_lastWriterChoice);
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
                saveReaderDropdown.RegisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                saveWriterDropdown.RegisterValueChangedCallback(OnWriterDropdownChoiceChanged);
                _refreshButton.clicked += Refresh;
            }
            else
            {
                storageSettings.UnregisterValueChangedCallback(OnStorageSettingsChanged);
                saveReaderDropdown.UnregisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                saveWriterDropdown.UnregisterValueChangedCallback(OnWriterDropdownChoiceChanged);
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
            AssignSelection(evt.newValue, readerTypeMap, readerInstanceMap,
                so =>
                {
                    _sysSettings.SaveReader = so as ISaveReader;
                    if (so != null)
                    {
                        _lastReaderChoice = saveReaderDropdown.value;
                        MakeSysSettingsChangesStick();
                    }
                });
        }

        private void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            AssignSelection(evt.newValue, writerTypeMap, writerInstanceMap,
                so =>
                {
                    _sysSettings.SaveWriter = so as ISaveWriter;
                    if (so != null)
                    {
                        _lastWriterChoice = saveWriterDropdown.value;
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
            ISaveDataApplier applierInstance = validMainApplierChoices[currentChoice];

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

        private static string space = " ", underscore = "_";

        private void OnBindItemForMainAppliers(VisualElement visElem, int index)
        {
            DropdownField dropdown = (DropdownField)visElem;
            dropdown.userData = index;
            dropdown.choices = validMainApplierChoices.Keys.ToList();
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

        #region Selection Handling Helpers

        private static string GetDisplayName(Type type)
        {
            var attr = type.GetCustomAttribute<SaveSysDisplayName>();
            if (attr != null)
            {
                return attr.DisplayName;
            }

            return $"{type.Name} ({type.Namespace})";
        }

        private void AssignSelection(
            string selectedChoice,
            IDictionary<string, Type> typeMap,
            IDictionary<TypeChoiceInfo, ScriptableObject> instanceMap,
            System.Action<ScriptableObject> applyAction)
        {
            if (string.IsNullOrEmpty(selectedChoice))
            {
                applyAction(null);
                return;
            }

            ScriptableObject instance = GetInstanceForChoice(selectedChoice, typeMap, instanceMap);
            applyAction(instance);
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
                if (_sysSettings.SaveReader == null && !string.IsNullOrEmpty(saveReaderDropdown?.value))
                {
                    var readerInstance = GetInstanceForChoice(saveReaderDropdown.value, readerTypeMap, readerInstanceMap);
                    _sysSettings.SaveReader = readerInstance as ISaveReader;
                }

                if (_sysSettings.SaveWriter == null && !string.IsNullOrEmpty(saveWriterDropdown?.value))
                {
                    var writerInstance = GetInstanceForChoice(saveWriterDropdown.value, writerTypeMap, writerInstanceMap);
                    _sysSettings.SaveWriter = writerInstance as ISaveWriter;
                }
            }

            RecordCurrentChoices();
        }

        private ScriptableObject GetInstanceForChoice(
            string choice,
            IDictionary<string, Type> typeMap,
            IDictionary<TypeChoiceInfo, ScriptableObject> instanceMap)
        {
            if (string.IsNullOrEmpty(choice) || !typeMap.TryGetValue(choice, out var concreteType))
            {
                return null;
            }

            foreach (var kvp in instanceMap)
            {
                if (kvp.Key.Type == concreteType)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        private void FillAppliersListBasedOnSettingsSO()
        {
            _mainApplierDropdowns.Clear();
            if (SysSettings != null)
            {
                int count = SysSettings.MainAppliers.Count;
                for (int i = 0; i < count; i++)
                {
                    var applier = SysSettings.MainAppliers[i];
                    string displayName = GetDisplayName(applier.GetType());
                    var dropdown = new DropdownField
                    {
                        value = displayName,
                        choices = validMainApplierChoices.Keys.ToList()
                    };
                    _mainApplierDropdowns.Add(dropdown);
                }
            }
            _mainAppliersView.MarkDirtyRepaint();
            _mainAppliersView.RefreshItems();
        }

        private void RecordCurrentChoices()
        {
            if (saveReaderDropdown != null && !string.IsNullOrEmpty(saveReaderDropdown.value))
            {
                _lastReaderChoice = saveReaderDropdown.value;
            }

            if (saveWriterDropdown != null && !string.IsNullOrEmpty(saveWriterDropdown.value))
            {
                _lastWriterChoice = saveWriterDropdown.value;
            }
        }

        private void Refresh()
        {
            SaveReaderTypeRegistry.DiscoverAndRegister();
            SaveWriterTypeRegistry.DiscoverAndRegister();
            SaveDataApplierRegistry.DiscoverAndRegister();
            
            UpdateCacheFromTypeRegistries();
            
            RegisterViews();
            RefreshDropdowns();
            PrepReaderAndWriterInstances();

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
                if (saveReaderDropdown.choices.Contains(choice))
                {
                    saveReaderDropdown.SetValueWithoutNotify(choice);
                    _lastReaderChoice = choice;
                }
            }

            if (SysSettings.SaveWriter != null)
            {
                var writerType = SysSettings.SaveWriter.GetType();
                string choice = GetDisplayName(writerType);
                if (saveWriterDropdown.choices.Contains(choice))
                {
                    saveWriterDropdown.SetValueWithoutNotify(choice);
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

        #region Nested Types

        public class TypeChoiceInfo
        {
            public Type Type { get; set; }
            public string ChoiceText { get; set; }
        }

        #endregion
    }
}