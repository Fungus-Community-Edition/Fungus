using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Handles population, persistence, and instance mapping for SaveSys dropdowns.
    /// Decouples dropdown logic from the EditorWindow orchestration.
    /// </summary>
    public class SaveSysDropdownController : IDisposable
    {
        #region Configuration
        private static readonly string settingsSubfolder = "SaveSys/Settings";
        #endregion

        /// <summary>
        /// Initializes the controller with the relevant UI elements and type cache.
        /// </summary>
        public virtual void Init(VisualElement root, SaveSysSettingsTypeCache typeCache)
        {
            _readerDropdown = root.Q<DropdownField>("SaveReaderDropdown");
            _writerDropdown = root.Q<DropdownField>("SaveWriterDropdown");
            _typeCache = typeCache;
        }

        private DropdownField _readerDropdown;
        private DropdownField _writerDropdown;
        private SaveSysSettingsTypeCache _typeCache;

        public DropdownField ReaderDropdown => _readerDropdown;
        public DropdownField WriterDropdown => _writerDropdown;

        /// <summary>
        /// Refreshes the dropdown options based on the current type cache.
        /// </summary>
        public virtual void Refresh()
        {
            RefreshTypeMap(_readerTypeMap, _typeCache.ReaderTypes);
            RefreshTypeMap(_writerTypeMap, _typeCache.WriterTypes);

            RefreshDropdown(_readerDropdown, _readerTypeMap);
            RefreshDropdown(_writerDropdown, _writerTypeMap);

            PrepReaderAndWriterInstances();
        }

        // The keys here are the display names.
        private readonly IDictionary<string, Type> _readerTypeMap = new Dictionary<string, Type>();
        private readonly IDictionary<string, Type> _writerTypeMap = new Dictionary<string, Type>();

        private static void RefreshTypeMap(IDictionary<string, Type> typeMap,
            IReadOnlyList<Type> validTypes)
        {
            typeMap.Clear();
            
            foreach (var type in validTypes)
            {
                // We assume that the valid types passed here are NOT test types, hence why we
                // don't validate that here.
                string name = SaveSysTypeUtils.GetDisplayName(type);
                typeMap[name] = type;
            }
        }

        private static void RefreshDropdown(DropdownField dropdown,
            IDictionary<string, Type> typeMap)
        {
            dropdown.choices.Clear();
            dropdown.choices = typeMap.Keys.ToList();
        }

        public virtual void PrepReaderAndWriterInstances()
        {
            _readerInstanceMap.Clear();
            _writerInstanceMap.Clear();

            RegisterDefaultsFor(_readerInstanceMap, iReaderType);
            RegisterDefaultsFor(_writerInstanceMap, iWriterType);

            GetOrGenerateAssetsFor(_readerInstanceMap, _typeCache.ReaderTypes);
            GetOrGenerateAssetsFor(_writerInstanceMap, _typeCache.WriterTypes);
        }

        private readonly IDictionary<TypeChoiceInfo, ScriptableObject> _readerInstanceMap =
            new Dictionary<TypeChoiceInfo, ScriptableObject>();
        private readonly IDictionary<TypeChoiceInfo, ScriptableObject> _writerInstanceMap =
            new Dictionary<TypeChoiceInfo, ScriptableObject>();

        private static readonly Type iReaderType = typeof(ISaveReader);
        private static readonly Type iWriterType = typeof(ISaveWriter);

        private static void RegisterDefaultsFor(IDictionary<TypeChoiceInfo, ScriptableObject> map, Type interfaceType)
        {
            const string defaultsSubfolder = AmanitaConstants.PathToSaveSysDefaultsFolder;
            var defaultInstances = Resources.LoadAll<ScriptableObject>(defaultsSubfolder)
                .Where(elem => elem != null && interfaceType.IsAssignableFrom(elem.GetType()))
                .ToList();

            foreach (var elem in defaultInstances)
            {
                var info = new TypeChoiceInfo
                {
                    Type = elem.GetType(),
                    ChoiceText = SaveSysTypeUtils.GetDisplayName(elem.GetType())
                };
                map[info] = elem;
            }
        }

        private static void GetOrGenerateAssetsFor(IDictionary<TypeChoiceInfo, ScriptableObject> map,
            IReadOnlyList<Type> validTypes)
        {
            foreach (var type in validTypes)
            {
                if (map.Keys.Any(keyEl => keyEl.Type == type))
                {
                    continue;
                }

                string displayName = SaveSysTypeUtils.GetDisplayName(type);
                displayName = displayName.Replace(space, underScore);
                string assetName = $"Generated{displayName}";
                var instance = SOUtils.GetOrCreateScriptableObject(type, settingsSubfolder, assetName);

                var choiceInfo = new TypeChoiceInfo
                {
                    Type = type,
                    ChoiceText = SaveSysTypeUtils.GetDisplayName(type)
                };
                map[choiceInfo] = instance;
            }
        }

        private static readonly string space = " ", underScore = "_";

        public virtual void AssignSelection(string selectedChoice, bool isReader, Action<ScriptableObject> applyAction)
        {
            if (string.IsNullOrEmpty(selectedChoice))
            {
                applyAction(null);
                return;
            }

            var instance = GetInstanceForChoice(selectedChoice, isReader);
            applyAction(instance);
        }

        public virtual ScriptableObject GetInstanceForChoice(string choice, bool isReader)
        {
            var typeMap = isReader ? _readerTypeMap : _writerTypeMap;
            var instanceMap = isReader ? _readerInstanceMap : _writerInstanceMap;

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

        public virtual void Dispose()
        {
            _readerDropdown = null;
            _writerDropdown = null;
            _readerInstanceMap.Clear();
            _writerInstanceMap.Clear();
        }

        public virtual void SetFrom(SaveSystemSettings sysSettings)
        {
            _readerDropdown.SetValueWithoutNotify("");
            _writerDropdown.SetValueWithoutNotify("");

            if (sysSettings.SaveReader != null)
            {
                var readerType = sysSettings.SaveReader.GetType();
                var readerDisplayName = SaveSysTypeUtils.GetDisplayName(readerType);
                if (_readerTypeMap.ContainsKey(readerDisplayName))
                {
                    _readerDropdown.SetValueWithoutNotify(readerDisplayName);
                }
            }

            if (sysSettings.SaveWriter != null)
            {
                var writerType = sysSettings.SaveWriter.GetType();
                var writerDisplayName = SaveSysTypeUtils.GetDisplayName(writerType);
                if (_writerTypeMap.ContainsKey(writerDisplayName))
                {
                    _writerDropdown.SetValueWithoutNotify(writerDisplayName);
                }
            }
        }
    }

}