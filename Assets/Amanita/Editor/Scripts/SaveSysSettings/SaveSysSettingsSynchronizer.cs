using System;
using UnityEditor;
using UnityEngine;

namespace Amanita.SaveSys.EditorUtils
{     
    /// <summary>
    /// Synchronizes SaveSystemSettings asset with the SaveSysSettingsWindow UI.
    /// Decouples synchronization logic from the window orchestration.
    /// </summary>
    public sealed class SaveSysSettingsSynchronizer : IDisposable
    {
        public void Init(SaveSystemSettings sysSettings,
                         SaveSysDropdownController dropdownController,
                         SaveSysSettingsUiRegistrar uiRegistrar)
        {
            _sysSettings = sysSettings;
            _dropdownController = dropdownController;
            _uiRegistrar = uiRegistrar;
        }

        private SaveSystemSettings _sysSettings;
        private SaveSysDropdownController _dropdownController;
        private SaveSysSettingsUiRegistrar _uiRegistrar;

        /// <summary>
        /// Apply changes to the backing asset and mark it dirty.
        /// </summary>
        public void MakeChangesStick()
        {
            if (_sysSettings == null) return;
            EditorUtility.SetDirty(_sysSettings);
            AssetDatabase.SaveAssetIfDirty(_sysSettings);
        }

        /// <summary>
        /// Fill missing asset settings based on current UI state.
        /// </summary>
        public void FillMissingAssetSettings()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null. Cannot fill missing asset settings.");
                return;
            }

            var readerDropdown = _uiRegistrar.ReaderDropdown;
            var writerDropdown = _uiRegistrar.WriterDropdown;

            if (_sysSettings.SaveReader == null && !string.IsNullOrEmpty(readerDropdown?.value))
            {
                var readerInstance = _dropdownController.GetInstanceForChoice(readerDropdown.value, isReader: true);
                _sysSettings.SaveReader = readerInstance as ISaveReader;
            }

            if (_sysSettings.SaveWriter == null && !string.IsNullOrEmpty(writerDropdown?.value))
            {
                var writerInstance = _dropdownController.GetInstanceForChoice(writerDropdown.value, isReader: false);
                _sysSettings.SaveWriter = writerInstance as ISaveWriter;
            }

            _uiRegistrar.RecordCurrentChoices();
        }

        /// <summary>
        /// Apply asset values back to the UI.
        /// </summary>
        public void ApplyAssetToUI()
        {
            if (_sysSettings == null)
            {
                Debug.LogWarning("SysSettings is null, cannot apply to UI.");
                return;
            }

            var storageSettingsView = _uiRegistrar.StorageSettings;
            storageSettingsView?.SetValueWithoutNotify(_sysSettings.StorageSettings);

            if (_sysSettings.SaveReader != null)
            {
                string choice = SaveSysTypeUtils.GetDisplayName(_sysSettings.SaveReader.GetType());
                var readerDropdown = _uiRegistrar.ReaderDropdown;
                if (readerDropdown.choices.Contains(choice))
                    readerDropdown.SetValueWithoutNotify(choice);
            }

            if (_sysSettings.SaveWriter != null)
            {
                string choice = SaveSysTypeUtils.GetDisplayName(_sysSettings.SaveWriter.GetType());
                var writerDropdown = _uiRegistrar.WriterDropdown;
                if (writerDropdown.choices.Contains(choice))
                    writerDropdown.SetValueWithoutNotify(choice);
            }
        }

        public void Dispose()
        {
            _sysSettings = null;
            _dropdownController = null;
            _uiRegistrar = null;
        }
    }
}