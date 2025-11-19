using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Type = System.Type;
using System.Reflection;
using UnityEditor.UIElements;
using System.Linq;

namespace Amanita.SaveSys.EditorUtils
{
    public class SaveSysSettingsWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

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
            PopulateDropdowns();

            PrepMapForReadersAndWriters();
            static void PrepMapForReadersAndWriters()
            {
                // So that when a choice changes, we can change the settings to an already-prepped instance.
                _readerInstanceMap.Clear();
                _writerInstanceMap.Clear();

                CheckDefaults();
                static void CheckDefaults()
                {
                    string defaultsSubfolder = AmanitaConstants.PathToSaveSysDefaultsFolder;
                    IList<ISaveReader> defaultReaders = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                        .Where((elem) => elem is ISaveReader)
                        .Cast<ISaveReader>()
                        .ToList();

                    foreach (var elem in defaultReaders)
                    {
                        _readerInstanceMap[elem.GetType()] = elem;
                    }

                    IList<ISaveWriter> defaultWriters = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                        .Where((elem) => elem is ISaveWriter)
                        .Cast<ISaveWriter>()
                        .ToList();

                    foreach (var elem in defaultWriters)
                    {
                        _writerInstanceMap[elem.GetType()] = elem;
                    }
                }

                string settingsSubfolder = "SaveSys/Settings";
                foreach (var readerType in SaveReaderTypeRegistry.ReaderTypes)
                {
                    if (_readerInstanceMap.ContainsKey(readerType))
                    {
                        continue; // Already have a default assigned
                    }

                    // We assume that all the reader types are either ScriptableObjects or
                    // concrete types with empty constructors.
                    ISaveReader readerInstance;
                    if (scriptableObjType.IsAssignableFrom(readerType))
                    {
                        string assetName = $"Generated{readerType.FullName}";
                        readerInstance = (ISaveReader)SOUtils.GetOrCreateScriptableObject(readerType, settingsSubfolder,
                            assetName);
                    }
                    else
                    {
                        readerInstance = (ISaveReader)System.Activator.CreateInstance(readerType);
                    }

                    string name = $"{readerType.Name} ({readerType.Namespace})";
                    _readerInstanceMap[readerType] = readerInstance;

                }

                // TODO: Implement SaveWriterTypeRegistry and do the same for writers
                foreach (var writerType in SaveWriterTypeRegistry.WriterTypes)
                {
                    if (_writerInstanceMap.ContainsKey(writerType))
                    {
                        continue; // Already have a default assigned
                    }
                    // We assume that all the writer types are either ScriptableObjects or
                    // concrete types with empty constructors.
                    ISaveWriter writerInstance;
                    if (scriptableObjType.IsAssignableFrom(writerType))
                    {
                        string assetName = $"Generated{writerType.FullName}";
                        writerInstance = (ISaveWriter)SOUtils.GetOrCreateScriptableObject(writerType, settingsSubfolder,
                            assetName);
                    }
                    else
                    {
                        writerInstance = (ISaveWriter)System.Activator.CreateInstance(writerType);
                    }
                    string name = $"{writerType.Name} ({writerType.Namespace})";
                    _writerInstanceMap[writerType] = writerInstance;
                }
            }
            ToggleSubs(true);
        }

        protected VisualElement Root => rootVisualElement;
        protected static Type scriptableObjType = typeof(ScriptableObject);
        protected static Type iSaveReaderType = typeof(ISaveReader);

        protected virtual void RegisterViews()
        {
            _saveReaderDropdown = Root.Q<DropdownField>("SaveReaderDropdown");
            _saveWriterDropdown = Root.Q<DropdownField>("SaveWriterDropdown");
            _storageSettings = Root.Q<ObjectField>("StorageSettings");
        }

        protected DropdownField _saveReaderDropdown, _saveWriterDropdown;
        protected ObjectField _storageSettings;

        protected virtual void PopulateDropdowns()
        {
            // Each viable type will get its own dropdown entry. This cuts down on the need for the
            // user to manually create and assign their own ScriptableObject types. In fact, this even
            // reduces the need to make certain things ScriptableObjects in the first place.
            foreach (var readerType in SaveReaderTypeRegistry.ReaderTypes)
            {
                string name = $"{readerType.Name} ({readerType.Namespace})";
                if (name.Contains("Test"))
                {
                    continue;
                }

                _readerTypeMap[name] = readerType;
                _saveReaderDropdown.choices.Add(name);

            }

            _saveReaderDropdown.value = _saveReaderDropdown.choices[0];

            foreach (var writerType in SaveWriterTypeRegistry.WriterTypes)
            {
                string name = $"{writerType.Name} ({writerType.Namespace})";
                if (name.Contains("Test"))
                {
                    continue;
                }
                _writerTypeMap[name] = writerType;
                _saveWriterDropdown.choices.Add(name);
            }

            _saveWriterDropdown.value = _saveWriterDropdown.choices[0];
        }

        protected static IDictionary<string, Type> _readerTypeMap = new Dictionary<string, Type>();
        protected static IDictionary<string, Type> _writerTypeMap = new Dictionary<string, Type>();

        protected static IDictionary<Type, ISaveReader> _readerInstanceMap = new Dictionary<Type, ISaveReader>(new TypeNameComparer());
        protected static IDictionary<Type, ISaveWriter> _writerInstanceMap = new Dictionary<Type, ISaveWriter>(new TypeNameComparer());
        // ^We have this map so we can easily change the reader or writer the SO uses

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                _storageSettings.RegisterValueChangedCallback(OnStorageSettingsChanged);
                _saveReaderDropdown.RegisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                _saveWriterDropdown.RegisterValueChangedCallback(OnWriterDropdownChoiceChanged);
            }
            else
            {
                _storageSettings.UnregisterValueChangedCallback(OnStorageSettingsChanged);
                _saveReaderDropdown.UnregisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                _saveWriterDropdown.UnregisterValueChangedCallback(OnWriterDropdownChoiceChanged);
            }
        }

        private void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            // TODO: Assign the selected writer type to the SysSettings
            throw new System.NotImplementedException();
        }

        private void OnReaderDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            // TODO: Assign the selected reader type to the SysSettings
            throw new System.NotImplementedException();
        }

        private void OnStorageSettingsChanged(ChangeEvent<Object> evt)
        {
            _sysSettings.StorageSettings = evt.newValue as SaveStorageSettings;
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
                _storageSettings.SetValueWithoutNotify(_sysSettings.StorageSettings);
                if (SysSettings.SaveReader != null)
                {
                    var readerType = SysSettings.SaveReader.GetType();
                    _saveReaderDropdown.SetValueWithoutNotify($"{readerType.Name} ({readerType.Namespace})");
                }

                if (SysSettings.SaveWriter != null)
                {
                    var writerType = SysSettings.SaveWriter.GetType();
                    _saveWriterDropdown.SetValueWithoutNotify($"{writerType.Name} ({writerType.Namespace})");
                }
            }

        }

    }
}