using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    /// <summary>
    /// Handles lookup and persistence of SaveSysSettingsWindow UI elements.
    /// Decouples view registration and choice restoration from the window orchestration.
    /// </summary>
    public class SaveSysSettingsUiRegistrar
    {
        private ObjectField _storageSettings;
        private Button _refreshButton;
        private DropdownField _readerDropdown;
        private DropdownField _writerDropdown;

        private string _lastReaderChoice;
        private string _lastWriterChoice;

        public virtual ObjectField StorageSettings => _storageSettings;
        public virtual Button RefreshButton => _refreshButton;
        public virtual DropdownField ReaderDropdown => _readerDropdown;
        public virtual DropdownField WriterDropdown => _writerDropdown;

        /// <summary>
        /// Finds and caches UI elements from the root visual tree.
        /// </summary>
        public virtual void Register(VisualElement root)
        {
            _storageSettings = root.Q<ObjectField>("StorageSettings");
            _refreshButton = root.Q<Button>("RefreshButton");
            _readerDropdown = root.Q<DropdownField>("SaveReaderDropdown");
            _writerDropdown = root.Q<DropdownField>("SaveWriterDropdown");
        }

        /// <summary>
        /// Restores previously recorded dropdown choices if they are still valid.
        /// </summary>
        public virtual void RestoreLastChoices()
        {
            if (_readerDropdown != null && !string.IsNullOrEmpty(_lastReaderChoice) &&
                _readerDropdown.choices.Contains(_lastReaderChoice))
            {
                _readerDropdown.SetValueWithoutNotify(_lastReaderChoice);
            }

            if (_writerDropdown != null && !string.IsNullOrEmpty(_lastWriterChoice) &&
                _writerDropdown.choices.Contains(_lastWriterChoice))
            {
                _writerDropdown.SetValueWithoutNotify(_lastWriterChoice);
            }
        }

        /// <summary>
        /// Records the current dropdown selections for persistence across window recreation.
        /// </summary>
        public virtual void RecordCurrentChoices()
        {
            if (_readerDropdown != null && !string.IsNullOrEmpty(_readerDropdown.value))
            {
                _lastReaderChoice = _readerDropdown.value;
            }

            if (_writerDropdown != null && !string.IsNullOrEmpty(_writerDropdown.value))
            {
                _lastWriterChoice = _writerDropdown.value;
            }
        }
    }
}