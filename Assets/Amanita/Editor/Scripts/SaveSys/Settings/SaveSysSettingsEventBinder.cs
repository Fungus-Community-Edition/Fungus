using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Centralizes event subscription logic for SaveSysSettingsWindow.
    /// Decouples callback wiring from the window orchestration.
    /// </summary>
    public sealed class SaveSysSettingsEventBinder
    {
        private SaveSystemSettings _sysSettings;
        private SaveSysDropdownController _dropdownController;
        private SaveSysSettingsSynchronizer _synchronizer;

        private ObjectField _storageSettings;
        private Button _refreshButton;

        public void Init(SaveSystemSettings sysSettings,
                         SaveSysDropdownController dropdownController,
                         SaveSysSettingsSynchronizer synchronizer,
                         ObjectField storageSettings,
                         Button refreshButton)
        {
            _sysSettings = sysSettings;
            _dropdownController = dropdownController;
            _synchronizer = synchronizer;
            _storageSettings = storageSettings;
            _refreshButton = refreshButton;
        }

        public void Toggle(bool on)
        {
            if (_storageSettings == null ||
                _dropdownController.ReaderDropdown == null ||
                _dropdownController.WriterDropdown == null ||
                _refreshButton == null)
            {
                return;
            }

            if (on)
            {
                _storageSettings.RegisterValueChangedCallback(OnStorageSettingsChanged);
                _dropdownController.ReaderDropdown.RegisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                _dropdownController.WriterDropdown.RegisterValueChangedCallback(OnWriterDropdownChoiceChanged);
                _refreshButton.clicked += OnRefreshClicked;
            }
            else
            {
                _storageSettings.UnregisterValueChangedCallback(OnStorageSettingsChanged);
                _dropdownController.ReaderDropdown.UnregisterValueChangedCallback(OnReaderDropdownChoiceChanged);
                _dropdownController.WriterDropdown.UnregisterValueChangedCallback(OnWriterDropdownChoiceChanged);
                _refreshButton.clicked -= OnRefreshClicked;
            }
        }

        internal void OnStorageSettingsChanged(ChangeEvent<Object> evt)
        {
            _sysSettings.StorageSettings = evt.newValue as SaveStorageSettings;
            _synchronizer.MakeChangesStick();
        }

        internal void OnReaderDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            _dropdownController.AssignSelection(evt.newValue, true, so =>
            {
                _sysSettings.SaveReader = so as ISaveReader;
                if (so != null)
                {
                    SaveSysSettingsWindow.LastReaderChoice = _dropdownController.ReaderDropdown.value;
                    _synchronizer.MakeChangesStick();
                }
            });
        }

        internal void OnWriterDropdownChoiceChanged(ChangeEvent<string> evt)
        {
            _dropdownController.AssignSelection(evt.newValue, false, so =>
            {
                _sysSettings.SaveWriter = so as ISaveWriter;
                if (so != null)
                {
                    SaveSysSettingsWindow.LastWriterChoice = _dropdownController.WriterDropdown.value;
                    _synchronizer.MakeChangesStick();
                }
            });
        }

        private void OnRefreshClicked()
        {
            // Delegate back to window orchestration if needed
            // Could be injected as an Action if you want to avoid direct coupling
        }
    }
}