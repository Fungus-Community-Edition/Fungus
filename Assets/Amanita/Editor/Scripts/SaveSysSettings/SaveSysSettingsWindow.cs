using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Amanita.SaveSys.EditorUtils
{
    public sealed class SaveSysSettingsWindow : EditorWindow
    {
        // Enforced single instance
        public static SaveSysSettingsWindow Instance { get; set; }

        // Persist prior UI selections across recreation
        private static string _lastReaderChoice;
        private static string _lastWriterChoice;

        private readonly SaveSysSettingsSynchronizer _synchronizer = new SaveSysSettingsSynchronizer();

        public static string LastReaderChoice
        {
            get => _lastReaderChoice;
            set => _lastReaderChoice = value;
        }
        public static string LastWriterChoice
        {
            get => _lastWriterChoice;
            set => _lastWriterChoice = value;
        }

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Atelier Mycelia/Amanita/Save Sys Settings")]
        public static void Open()
        {
            if (Instance != null)
            {
                Instance._lifecycle.HandleOnRefresh(Instance);
                // No more need for setup (remember, we want only one instance active at a time), so...
                return;
            }

            // GetWindow will reuse an existing one of the same type if present.
            SaveSysSettingsWindow wnd = GetWindow<SaveSysSettingsWindow>();
            wnd.titleContent = new GUIContent("Save Sys Settings");

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
            wnd.Refresh();
            wnd.Focus();
        }

        private void OnEnable()
        {
            _lifecycle.HandleOnEnable(this);
            if (_uiIsReadyForAccess)
            {
                Refresh();
            }
        }

        private void OnDestroy()
        {
            _lifecycle.HandleOnDestroy(this, _uiRegistrar);
            Instance = null;
        }

        /// <summary>
        /// Executes every time the window opens, prepping its gui for the world to see.
        /// </summary>
        public void CreateGUI()
        {
            VisualElement mainUxml = m_VisualTreeAsset.Instantiate();
            Root.Add(mainUxml);
            _uiIsReadyForAccess = true;

            _typeCache.Refresh();

            _dropdownController.Init(Root, _typeCache);
            _mainAppliersController.Init(Root, _typeCache);
            _uiRegistrar.Register(Root);

            _dropdownController.Refresh();
            _synchronizer.Init(SysSettings, _dropdownController, _uiRegistrar);
            _eventBinder.Init(SysSettings, _dropdownController, _synchronizer,
                _uiRegistrar.StorageSettings, _uiRegistrar.RefreshButton);

            _uiRegistrar.RestoreLastChoices();
            _eventBinder.Toggle(true);
            _synchronizer.FillMissingAssetSettings();
        }


        private bool _uiIsReadyForAccess = false;
        private VisualElement Root => rootVisualElement;

        private readonly SaveSysDropdownController _dropdownController = new SaveSysDropdownController();
        private readonly SaveSysMainAppliersController _mainAppliersController = new SaveSysMainAppliersController();

        private static readonly SaveSysSettingsTypeCache _typeCache = new SaveSysSettingsTypeCache();

        #region SaveSystemSettings Synchronization

        private void Refresh()
        {
            _typeCache.Refresh();
            _lifecycle.HandleOnRefresh(this);

            if (!_uiIsReadyForAccess) // For when called before CreateGUI
            {
                return;
            }

            _dropdownController.Refresh();
            _dropdownController.SetFrom(SysSettings);
            _mainAppliersController.BindToSettings(SysSettings);
            
            var storageSettingsView = _uiRegistrar.StorageSettings;
            storageSettingsView.SetValueWithoutNotify(SysSettings.StorageSettings);

            _eventBinder.Toggle(false);
            _mainAppliersController.ToggleSubs(false);
            _eventBinder.Toggle(true);
            _mainAppliersController.ToggleSubs(true);
            _synchronizer.ApplyAssetToUI();
        }

        private SaveSystemSettings SysSettings
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
        private SaveSystemSettings _sysSettings;

        private readonly SaveSysSettingsEventBinder _eventBinder = new SaveSysSettingsEventBinder();
        private readonly SaveSysSettingsUiRegistrar _uiRegistrar = new SaveSysSettingsUiRegistrar();
        private readonly SaveSysSettingsLifecycleManager _lifecycle = new SaveSysSettingsLifecycleManager();

        private void OnDisable()
        {
            _eventBinder.Toggle(false);
            _mainAppliersController.ToggleSubs(false);
            _synchronizer.Dispose();
            _dropdownController.Dispose();
            _uiIsReadyForAccess = false;
        }
    
        #endregion

    }
}