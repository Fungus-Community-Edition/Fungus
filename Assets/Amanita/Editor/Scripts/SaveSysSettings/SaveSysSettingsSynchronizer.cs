using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Synchronizes SaveSystemSettings asset state with UI selections.
    /// Decouples persistence logic from the EditorWindow orchestration.
    /// </summary>
    public sealed class SaveSysSettingsSynchronizer : IDisposable
    {
        public void Init(SaveSystemSettings sysSettings,
                                           SaveSysDropdownController dropdownController)
        {
            _sysSettings = sysSettings;
            _dropdownController = dropdownController;
        }

        private SaveSystemSettings _sysSettings;
        private SaveSysDropdownController _dropdownController;

        /// <summary>
        /// Apply changes to the backing asset and mark it dirty.
        /// </summary>
        public void MakeChangesStick()
        {
            if (_sysSettings == null)
            {
                return;
            }

            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        /// <summary>
        /// Fill missing asset settings based on current UI state.
        /// Ensures reader/writer instances are assigned if dropdowns have values.
        /// </summary>
        public void FillMissingAssetSettings(DropdownField readerDropdown, DropdownField writerDropdown)
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null. Cannot fill missing asset settings.");
                return;
            }

            bool readerDropdownHasValue = !string.IsNullOrEmpty(readerDropdown?.value);
            if (_sysSettings.SaveReader == null && readerDropdownHasValue)
            {
                var readerInstance = _dropdownController.GetInstanceForChoice(readerDropdown.value, isReader: true);
                _sysSettings.SaveReader = readerInstance as ISaveReader;
            }

            bool writerDropdownHasValue = !string.IsNullOrEmpty(writerDropdown?.value);
            if (_sysSettings.SaveWriter == null && writerDropdownHasValue)
            {
                var writerInstance = _dropdownController.GetInstanceForChoice(writerDropdown.value, isReader: false);
                _sysSettings.SaveWriter = writerInstance as ISaveWriter;
            }

            RecordCurrentChoices(readerDropdown, writerDropdown);
        }

        /// <summary>
        /// Apply asset values back to the UI (storage settings, dropdowns).
        /// </summary>
        public void ApplyAssetToUI(ObjectField storageSettings,
                                   DropdownField readerDropdown,
                                   DropdownField writerDropdown)
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null, cannot apply to UI.");
                return;
            }

            storageSettings?.SetValueWithoutNotify(_sysSettings.StorageSettings);

            if (_sysSettings.SaveReader != null)
            {
                string choice = SaveSysTypeUtils.GetDisplayName(_sysSettings.SaveReader.GetType());
                if (readerDropdown.choices.Contains(choice))
                {
                    readerDropdown.SetValueWithoutNotify(choice);
                }
            }

            if (_sysSettings.SaveWriter != null)
            {
                string choice = SaveSysTypeUtils.GetDisplayName(_sysSettings.SaveWriter.GetType());
                if (writerDropdown.choices.Contains(choice))
                {
                    writerDropdown.SetValueWithoutNotify(choice);
                }
            }
        }

        /// <summary>
        /// Record current dropdown choices for persistence across reloads.
        /// </summary>
        private void RecordCurrentChoices(DropdownField readerDropdown, DropdownField writerDropdown)
        {
            if (readerDropdown != null && !string.IsNullOrEmpty(readerDropdown.value))
            {
                SaveSysSettingsWindow.LastReaderChoice = readerDropdown.value;
            }

            if (writerDropdown != null && !string.IsNullOrEmpty(writerDropdown.value))
            {
                SaveSysSettingsWindow.LastWriterChoice = writerDropdown.value;
            }
        }

        public void Dispose()
        {
            _sysSettings = null;
            _dropdownController = null;
        }
    }
}