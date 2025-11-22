using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Type = System.Type;
using UnityEditor.UIElements;
using System.Linq;
using Collections;
using System.Reflection;

namespace Amanita.SaveSys.EditorUtils
{
    public class SaveSysSettingsWindow : EditorWindow
    {
        [SerializeField]
        protected VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Amanita/Save Sys Settings")]
        public static void Open()
        {
            SaveSysSettingsWindow wnd = GetWindow<SaveSysSettingsWindow>();
            wnd.titleContent = new GUIContent("Save Sys Settings");
            wnd.minSize = wnd.maxSize = windowSize;

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

            wnd.SysSettings = settings; // Runs after CreateGUI
        }

        protected static Vector2 windowSize = new Vector2(600, 700);

        public virtual void CreateGUI()
        {
            VisualElement mainUxml = m_VisualTreeAsset.Instantiate();
            Root.Add(mainUxml);
            RegisterViews();
            UpdateCacheFromTypeRegistries();
            RefreshDropdowns();
            PrepReaderAndWriterInstances();
            ToggleSubs(true);
            FillMissingAssetSettingsBasedOnUi();
        }

        protected VisualElement Root => rootVisualElement;
        protected static Type scriptableObjType = typeof(ScriptableObject);
        protected static Type iSaveReaderType = typeof(ISaveReader);

        #region UI Setup / Registration

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

        #endregion

        #region Type Discovery / Caches

        protected virtual void UpdateCacheFromTypeRegistries()
        {
            validReaderTypes.Clear();
            validWriterTypes.Clear();

            // For the sake of easier persistence (and reduced headache), only
            // allow ScriptableObject-based readers/writers.
            IList<Type> scriptableObjReaders = SaveReaderTypeRegistry.ReaderTypes
                .Where(typeEl => !typeEl.Name.Contains("Test") &&
                scriptableObjType.IsAssignableFrom(typeEl)
                    && iSaveReaderType.IsAssignableFrom(typeEl)).ToList();
            validReaderTypes.AddRange(scriptableObjReaders);

            IList<Type> scriptableObjWriters = SaveWriterTypeRegistry.WriterTypes
                .Where(typeEl => !typeEl.Name.Contains("Test") &&
                scriptableObjType.IsAssignableFrom(typeEl)
                    && typeof(ISaveWriter).IsAssignableFrom(typeEl)).ToList();
            validWriterTypes.AddRange(scriptableObjWriters);
        }

        protected static IList<Type> validReaderTypes = new List<Type>();
        protected static IList<Type> validWriterTypes = new List<Type>();

        #endregion

        #region Instance Provisioning

        static void PrepReaderAndWriterInstances()
        {
            // So that when a choice changes, we can instantly change the settings
            // to an already-prepped instance.
            readerInstanceMap.Clear();
            writerInstanceMap.Clear();

            RegisterDefaultsFor(readerInstanceMap, typeof(ISaveReader));
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
            RegisterDefaultsFor(writerInstanceMap, typeof(ISaveWriter));

            GetOrGenerateAssetsFor(readerInstanceMap, validReaderTypes);
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
            GetOrGenerateAssetsFor(writerInstanceMap, validWriterTypes);
        }

        #endregion

        #region Dropdown Population

        protected virtual void RefreshDropdowns()
        {
            readerTypeMap.Clear();
            writerTypeMap.Clear();
            saveReaderDropdown.choices.Clear();
            saveWriterDropdown.choices.Clear();

            Populate(readerTypeMap, validReaderTypes, saveReaderDropdown);
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
            Populate(writerTypeMap, validWriterTypes, saveWriterDropdown);

            
            // Let later logic decide initial selections (do not assign here).
        }

        protected static IDictionary<string, Type> readerTypeMap = new Dictionary<string, Type>();
        protected static IDictionary<string, Type> writerTypeMap = new Dictionary<string, Type>();

        protected static IDictionary<TypeChoiceInfo, ScriptableObject> readerInstanceMap = new Dictionary<TypeChoiceInfo, ScriptableObject>();
        protected static IDictionary<TypeChoiceInfo, ScriptableObject> writerInstanceMap = new Dictionary<TypeChoiceInfo, ScriptableObject>();

        #endregion

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
                        MakeSysSettingsChangesStick();
                    }
                });
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

        protected virtual ScriptableObject GetInstanceForChoice(
            string choice,
            IDictionary<string, Type> typeMap,
            IDictionary<TypeChoiceInfo, ScriptableObject> instanceMap)
        {
            ScriptableObject result = null;
            Type concreteType = null;
            bool validChoice = !string.IsNullOrEmpty(choice) && typeMap.TryGetValue(choice, out concreteType);
            if (validChoice)
            {
                // Avoid repeated LINQ by simple loop.
                foreach (var kvp in instanceMap)
                {
                    if (kvp.Key.Type == concreteType)
                    {
                        return kvp.Value;
                    }
                }
            }
            return result;
        }

        protected virtual void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            AssignSelection(evt.newValue, writerTypeMap, writerInstanceMap,
                so =>
                {
                    _sysSettings.SaveWriter = so as ISaveWriter;
                    if (so != null)
                    {
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

        #endregion

        #region SaveSystemSettings Synchronization

        protected virtual void MakeSysSettingsChangesStick()
        {
            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        protected virtual void FillMissingAssetSettingsBasedOnUi()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null, cannot fill missing asset settings.");
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

            void ApplySettingsAssetToUI()
            {
                if (_sysSettings == null)
                {
                    Debug.LogWarning("SysSettings is null, cannot apply to UI.");
                    return;
                }

                storageSettings.SetValueWithoutNotify(_sysSettings.StorageSettings);

                if (SysSettings.SaveReader != null)
                {
                    saveReaderDropdown.SetValueWithoutNotify(GetDisplayName(SysSettings.SaveReader.GetType()));
                }

                if (SysSettings.SaveWriter != null)
                {
                    saveWriterDropdown.SetValueWithoutNotify(GetDisplayName(SysSettings.SaveWriter.GetType()));
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