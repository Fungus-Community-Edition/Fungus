using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Type = System.Type;
using UnityEditor.UIElements;
using System.Linq;
using Collections;

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

            wnd.SysSettings = settings; // Note that this runs after CreateGUI

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
        }

        protected virtual void UpdateCacheFromTypeRegistries()
        {
            validReaderTypes.Clear();
            validWriterTypes.Clear();

            // For the sake of easier persistence (and reduced headache), we'll only work with types that not only
            // implement the right interface, but also inherit from ScriptableObject.
            IList<Type> scriptableObjReaders = SaveReaderTypeRegistry.ReaderTypes
                .Where((elem) => scriptableObjType.IsAssignableFrom(elem)
                    && iSaveReaderType.IsAssignableFrom(elem)).ToList();
            validReaderTypes.AddRange(scriptableObjReaders);

            IList<Type> scriptableObjWriters = SaveWriterTypeRegistry.WriterTypes
                .Where((elem) => scriptableObjType.IsAssignableFrom(elem)
                    && typeof(ISaveWriter).IsAssignableFrom(elem)).ToList();
            validWriterTypes.AddRange(scriptableObjWriters);
        }

        protected static IList<Type> validReaderTypes = new List<Type>();
        protected static IList<Type> validWriterTypes = new List<Type>();

        static void PrepReaderAndWriterInstances()
        {
            // So that when a choice changes, we can change the settings to an already-prepped instance.
            readerInstanceMap.Clear();
            writerInstanceMap.Clear();

            RegisterDefaultsFor(readerInstanceMap);

            static IList<Type> RegisterDefaultsFor<T>(IDictionary<TypeChoiceInfo, T> map)
            {
                // The defaults we'll consider here are all ScriptableObjects.
                IList<Type> result = new List<Type>();
                const string defaultsSubfolder = AmanitaConstants.PathToSaveSysDefaultsFolder;
                IList<T> defaultInstances = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                    .Where((elem) => elem is T)
                    .Cast<T>()
                    .ToList();

                foreach (var elem in defaultInstances)
                {
                    Type elemType = elem.GetType();
                    result.Add(elemType);
                    TypeChoiceInfo info = new TypeChoiceInfo
                    {
                        Type = elemType,
                        ChoiceText = $"{elemType.Name} ({elemType.Namespace})"
                    };
                    map[info] = elem;
                }

                return result;
            }

            RegisterDefaultsFor(writerInstanceMap);

            GetOrGenerateAssetsFor(readerInstanceMap, validReaderTypes);
            void GetOrGenerateAssetsFor<T>(IDictionary<TypeChoiceInfo, T> map, IList<Type> validTypes) where T: class
            {
                string settingsSubfolder = "SaveSys/Settings";
                foreach (var type in validTypes)
                {
                    bool alreadyRegisteredForThisType = map.Keys
                        .Where((elem) => elem.Type.Equals(type)).Any();
                    if (alreadyRegisteredForThisType)
                    {
                        continue; // We only want one instance per concrete type.
                    }
                    T instance;
                    string assetName = $"Generated{type.FullName}";
                    instance = SOUtils.GetOrCreateScriptableObject(type, settingsSubfolder,
                        assetName) as T;
                    string name = $"{type.Name} ({type.Namespace})";
                    TypeChoiceInfo choiceInfo = new TypeChoiceInfo
                    {
                        Type = type,
                        ChoiceText = name
                    };
                    map[choiceInfo] = instance;
                }
            }
            GetOrGenerateAssetsFor(writerInstanceMap, validWriterTypes);

        }

        protected VisualElement Root => rootVisualElement;
        protected static Type scriptableObjType = typeof(ScriptableObject);
        protected static Type iSaveReaderType = typeof(ISaveReader);

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

        protected virtual void RefreshDropdowns()
        {
            // Each viable type will get its own dropdown entry. This cuts down on the need for the
            // user to manually create and assign their own ScriptableObject types. In fact, this even
            // reduces the need to make certain things ScriptableObjects in the first place.
            readerTypeMap.Clear();
            writerTypeMap.Clear();
            saveReaderDropdown.choices.Clear();
            saveWriterDropdown.choices.Clear();

            Populate(readerTypeMap, validReaderTypes, saveReaderDropdown);
            static void Populate(IDictionary<string, Type> map, IList<Type> validTypes, DropdownField dropdown)
            {
                foreach (var type in validTypes)
                {
                    // In the future, we might want to add an attribute that lets the classes decide their
                    // display names in this window.
                    string name = $"{type.Name} ({type.Namespace})";
                    if (name.Contains("Test"))
                    {
                        continue;
                    }
                    map[name] = type;
                    dropdown.choices.Add(name);
                }
            }
            Populate(writerTypeMap, validWriterTypes, saveWriterDropdown);

            saveReaderDropdown.value = saveReaderDropdown.choices[0];
            saveWriterDropdown.value = saveWriterDropdown.choices[0];
        }

        // The key is the choice text shown in the dropdown.
        protected static IDictionary<string, Type> readerTypeMap = new Dictionary<string, Type>();
        protected static IDictionary<string, Type> writerTypeMap = new Dictionary<string, Type>();

        protected static IDictionary<TypeChoiceInfo, ISaveReader> readerInstanceMap = new Dictionary<TypeChoiceInfo, ISaveReader>();
        protected static IDictionary<TypeChoiceInfo, ISaveWriter> writerInstanceMap = new Dictionary<TypeChoiceInfo, ISaveWriter>();
        // ^We have this map so we can easily change the reader or writer the SO uses

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

        protected virtual void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            // At this point, we should have all the appropriate instances prepped in writerInstanceMap.
            string selectedChoice = evt.newValue;
            Type typeLinkedToChoice = writerTypeMap[selectedChoice];
            TypeChoiceInfo choiceInfo = writerInstanceMap.Keys
                .Where((elem) => elem.Type.Equals(typeLinkedToChoice))
                .FirstOrDefault();
            if (choiceInfo != null && writerInstanceMap.ContainsKey(choiceInfo))
            {
                ISaveWriter writerToAssign = writerInstanceMap[choiceInfo];
                _sysSettings.SaveWriter = writerToAssign;
                MakeSysSettingsChangesStick();
            }
            
        }

        protected virtual void MakeSysSettingsChangesStick()
        {
            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        protected virtual void OnReaderDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            string selectedChoice = evt.newValue;
            Type typeLinkedToChoice = readerTypeMap[selectedChoice];
            TypeChoiceInfo choiceInfo = readerInstanceMap.Keys
                .Where((elem) => elem.Type.Equals(typeLinkedToChoice))
                .FirstOrDefault();
            if (choiceInfo != null && readerInstanceMap.ContainsKey(choiceInfo))
            {
                ISaveReader readerToAssign = readerInstanceMap[choiceInfo];
                _sysSettings.SaveReader = readerToAssign;
                MakeSysSettingsChangesStick();
            }
        }

        protected virtual void OnStorageSettingsChanged(ChangeEvent<Object> evt)
        {
            _sysSettings.StorageSettings = evt.newValue as SaveStorageSettings;
            MakeSysSettingsChangesStick();
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

                // Set Storage Settings
                storageSettings.SetValueWithoutNotify(_sysSettings.StorageSettings);
                if (SysSettings.SaveReader != null)
                {
                    var readerType = SysSettings.SaveReader.GetType();
                    saveReaderDropdown.SetValueWithoutNotify($"{readerType.Name} ({readerType.Namespace})");
                }

                if (SysSettings.SaveWriter != null)
                {
                    var writerType = SysSettings.SaveWriter.GetType();
                    saveWriterDropdown.SetValueWithoutNotify($"{writerType.Name} ({writerType.Namespace})");
                }
            }

        }

        public class TypeChoiceInfo
        {
            public Type Type { get; set; }
            public string ChoiceText { get; set; }
        }

    }

    
}