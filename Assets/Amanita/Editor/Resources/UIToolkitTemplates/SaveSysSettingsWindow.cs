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
            SaveSystemSettings GetSysSettings()
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
            void PrepMapForReadersAndWriters()
            {
                _readerInstanceMap.Clear();
                _writerInstanceMap.Clear();

                void CheckDefaults()
                {
                    string defaultsSubfolder = "SaveSys/Defaults";
                    IList<ISaveReader> defaultSOs = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                        .Where((elem) => elem is ISaveReader)
                        .Cast<ISaveReader>()
                        .ToList();

                    foreach (var so in defaultSOs)
                    {
                        
                        string name = $"{so.GetType().Name} ({so.GetType().Namespace})";
                        _readerInstanceMap[name] = so;
                    }
                }
                string settingsSubfolder = "SaveSys/Settings";
                foreach (var readerType in SaveReaderTypeRegistry.ReaderTypes)
                {
                    if (_readerInstanceMap.ContainsKey(readerType.FullName))
                    {
                        continue;
                    }

                    // We assume that all the reader types are either ScriptableObjects or
                    // concrete types with empty constructors.
                    ISaveReader readerInstance;
                    if (soType.IsAssignableFrom(readerType))
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
                    _readerInstanceMap[name] = readerInstance;

                }

                // TODO: Implement the save writer type registry and uncomment this
                //foreach (var writerType in SaveWriterTypeRegistry.WriterTypes)
                //{
                //    if (soType.IsAssignableFrom(writerType) && writerType.GetConstructor(Type.EmptyTypes) != null)
                //    {
                //        string name = $"{writerType.Name} ({writerType.Namespace})";
                //        _writerInstanceMap[name] = writerType;
                //    }
                //}
            }
            ToggleSubs(true);
        }

        protected VisualElement Root => rootVisualElement;
        protected static Type soType = typeof(ScriptableObject);
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
        }

        protected static IDictionary<string, Type> _readerTypeMap = new Dictionary<string, Type>();

        protected static IDictionary<string, ISaveReader> _readerInstanceMap = new Dictionary<string, ISaveReader>();
        protected static IDictionary<string, Type> _writerInstanceMap = new Dictionary<string, Type>();
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