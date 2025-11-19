using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Type = System.Type;
using UnityEditor.UIElements;
using System.Linq;
using System.Reflection;
using Collections;

namespace Amanita.SaveSys.EditorUtils
{
    public class SaveSysSettingsWindow : EditorWindow
    {
        // Enforced single instance
        public static SaveSysSettingsWindow Instance { get; set; }

        // Persist prior UI selections across recreation
        protected static string _lastReaderChoice;
        protected static string _lastWriterChoice;

        [SerializeField]
        protected VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Amanita/Save Sys Settings")]
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
            wnd.Focus();
        }

        protected static Vector2 windowSize = new Vector2(600, 700);

        protected void EnsureSingleInstance()
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

        protected virtual void OnEnable()
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
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                // Record current choices before clearing instance.
                RecordCurrentChoices();
                Instance = null;
            }
        }

        public virtual void CreateGUI()
        {
            VisualElement mainUxml = m_VisualTreeAsset.Instantiate();
            Root.Add(mainUxml);
            RegisterViews();
            UpdateCacheFromTypeRegistries();
            RefreshDropdowns();
            PrepReaderAndWriterInstances();
            RestoreLastChoicesIfAny();
            ToggleSubs(true);
            FillMissingAssetSettingsBasedOnUi();
        }

        protected VisualElement Root => rootVisualElement;

        protected virtual void RegisterViews()
        {
            saveReaderDropdown = Root.Q<DropdownField>("SaveReaderDropdown");
            saveWriterDropdown = Root.Q<DropdownField>("SaveWriterDropdown");
            storageSettings = Root.Q<ObjectField>("StorageSettings");
            _refreshButton = Root.Q<Button>("RefreshButton");
        }

        protected DropdownField saveReaderDropdown, saveWriterDropdown;
        protected ObjectField storageSettings;
        protected Button _refreshButton;

        protected virtual void UpdateCacheFromTypeRegistries()
        {
            validReaderTypes.Clear();
            validWriterTypes.Clear();

            IList<Type> scriptableObjReaders = SaveReaderTypeRegistry.ReaderTypes
                .Where(typeEl => !typeEl.Name.Contains("Test") &&
                                 scriptableObjType.IsAssignableFrom(typeEl) &&
                                 iSaveReaderType.IsAssignableFrom(typeEl)).ToList();
            validReaderTypes.AddRange(scriptableObjReaders);

            IList<Type> scriptableObjWriters = SaveWriterTypeRegistry.WriterTypes
                .Where(typeEl => !typeEl.Name.Contains("Test") &&
                                 scriptableObjType.IsAssignableFrom(typeEl) &&
                                 typeof(ISaveWriter).IsAssignableFrom(typeEl)).ToList();
            validWriterTypes.AddRange(scriptableObjWriters);
        }

        protected static IList<Type> validReaderTypes = new List<Type>();
        protected static IList<Type> validWriterTypes = new List<Type>();

        protected static Type scriptableObjType = typeof(ScriptableObject);
        protected static Type iSaveReaderType = typeof(ISaveReader);
        protected static Type iSaveWriterType = typeof(ISaveWriter);

        protected virtual void RefreshDropdowns()
        {
            readerTypeMap.Clear();
            writerTypeMap.Clear();
            saveReaderDropdown.choices.Clear();
            saveWriterDropdown.choices.Clear();

            Populate(readerTypeMap, validReaderTypes, saveReaderDropdown);
            Populate(writerTypeMap, validWriterTypes, saveWriterDropdown);

            static void Populate(IDictionary<string, Type> map, IList<Type> validTypes, DropdownField dropdown)
            {
                foreach (var type in validTypes)
                {
                    string name = GetDisplayName(type);
                    if (name.Contains("Test"))
                    {
                        continue;
                    }
                    map[name] = type;
                    dropdown.choices.Add(name);
                }
            }
            // Do not assign initial values here.
        }

        protected static IDictionary<string, Type> readerTypeMap = new Dictionary<string, Type>();
        protected static IDictionary<string, Type> writerTypeMap = new Dictionary<string, Type>();

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

        protected static IDictionary<TypeChoiceInfo, ScriptableObject> readerInstanceMap = new Dictionary<TypeChoiceInfo, ScriptableObject>();
        protected static IDictionary<TypeChoiceInfo, ScriptableObject> writerInstanceMap = new Dictionary<TypeChoiceInfo, ScriptableObject>();

        protected virtual void RestoreLastChoicesIfAny()
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

        protected virtual void ToggleSubs(bool on)
        {
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
        }

        protected virtual void OnStorageSettingsChanged(ChangeEvent<Object> evt)
        {
            _sysSettings.StorageSettings = evt.newValue as SaveStorageSettings;
            MakeSysSettingsChangesStick();
        }

        protected virtual void OnReaderDropdownChoiceChanged(ChangeEvent<string> evt)
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

        protected virtual void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
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

        #endregion

        #region Selection Handling Helpers

        protected static string GetDisplayName(Type type)
        {
            var attr = type.GetCustomAttribute<SaveSysDisplayName>();
            if (attr != null)
            {
                return attr.DisplayName;
            }

            return $"{type.Name} ({type.Namespace})";
        }

        protected virtual void AssignSelection(
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

        protected virtual void MakeSysSettingsChangesStick()
        {
            if (_sysSettings == null)
            {
                return;
            }
            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        protected virtual void FillMissingAssetSettingsBasedOnUi()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null. Cannot fill missing asset settings.");
                return;
            }

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

            RecordCurrentChoices();
        }

        protected virtual ScriptableObject GetInstanceForChoice(
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

        protected virtual void RecordCurrentChoices()
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

        protected virtual void Refresh()
        {
            SaveReaderTypeRegistry.DiscoverAndRegister();
            SaveWriterTypeRegistry.DiscoverAndRegister();
            UpdateCacheFromTypeRegistries();
            RefreshDropdowns();
            PrepReaderAndWriterInstances();

            ToggleSubs(false);
            ToggleSubs(true);

            ApplySettingsAssetToUI();
        }

        protected virtual void ApplySettingsAssetToUI()
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
        }

        protected SaveSystemSettings SysSettings
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
        protected SaveSystemSettings _sysSettings;

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