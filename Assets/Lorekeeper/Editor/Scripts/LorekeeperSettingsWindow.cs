using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Lorekeeper.EditorCode
{
    public class LorekeeperSettingsWindow : EditorWindow
    {
        [SerializeField]
        protected VisualTreeAsset m_VisualTreeAsset = default;

        [MenuItem("Window/Lorekeeper/Settings")]
        public static void InitWindow()
        {
            LKUtils.EnsureWeHaveResourcesFolder();

            LorekeeperSettingsWindow window = GetWindow<LorekeeperSettingsWindow>();
            window.minSize = _windowSize;
            window.maxSize = _windowSize + new Vector2(1, 1);
            window.titleContent = new GUIContent("LorekeeperSettings");
            window.Focus();
        }

        protected static Vector2 _windowSize = new Vector2(600, 700);
        
        public void CreateGUI()
        {
            // Note that this function runs right when this is instantiated, meaning we can't simply assign
            // the settings right after. Thus, we'll load it here before anything else.
            Settings = settingsFactory.GetSettings();
            
            VisualElement root = rootVisualElement;
            m_VisualTreeAsset.CloneTree(root);

            RegisterControls();
            void RegisterControls()
            {
                assetsPathFieldHolder = root.Q<ListView>("RelativeToAssetsPathListView");
                addPathButton = root.Q<Button>("AddPathButton");
                saveButton = root.Q<Button>("SaveButton");

                trackAudioClipsToggle = root.Q<Toggle>("TrackAudioClipsToggle");
                trackAudioMixersToggle = root.Q<Toggle>("TrackAudioMixersToggle");
                trackSpritesToggle = root.Q<Toggle>("TrackSpritesToggle");
                trackTexturesToggle = root.Q<Toggle>("TrackTexturesToggle");
                trackRenderTexturesToggle = root.Q<Toggle>("TrackRenderTexturesToggle");
                trackCubemapsToggle = root.Q<Toggle>("TrackCubemapsToggle");
                trackMaterialsToggle = root.Q<Toggle>("TrackMaterialsToggle");
                trackShadersToggle = root.Q<Toggle>("TrackShadersToggle");
                trackComputeShadersToggle = root.Q<Toggle>("TrackComputeShadersToggle");
                trackAnimationClipsToggle = root.Q<Toggle>("TrackAnimationClipsToggle");
                trackAnimatorControllersToggle = root.Q<Toggle>("TrackAnimatorControllersToggle");
                trackAvatarsToggle = root.Q<Toggle>("TrackAvatarsToggle");
                trackModelsToggle = root.Q<Toggle>("TrackModelsToggle");
                trackMeshesToggle = root.Q<Toggle>("TrackMeshesToggle");
                trackPrefabsToggle = root.Q<Toggle>("TrackPrefabsToggle");
                trackFontsToggle = root.Q<Toggle>("TrackFontsToggle");
                trackTmpFontAssetsToggle = root.Q<Toggle>("TrackTmpFontAssetsToggle");
                trackScriptableObjectsToggle = root.Q<Toggle>("TrackScriptableObjectsToggle");
                trackTextAssetsToggle = root.Q<Toggle>("TrackTextAssetsToggle");
                trackPhysicsMaterialsToggle = root.Q<Toggle>("TrackPhysicsMaterialsToggle");
                trackPhysicsMaterials2DToggle = root.Q<Toggle>("TrackPhysicsMaterials2DToggle");
                trackOtherToggle = root.Q<Toggle>("TrackOtherToggle");
            }

            ToggleSubs(true);

            ConfigListViews();
            void ConfigListViews()
            {
                blacklistCopy.Clear();
                blacklistCopy.AddRange(Settings.Blacklist);
                assetsPathFieldHolder.itemsSource = blacklistCopy;
                assetsPathFieldHolder.reorderable = true;
                assetsPathFieldHolder.reorderMode = ListViewReorderMode.Animated;

                // Important: match the row height to your item’s USS height (36px)
                assetsPathFieldHolder.fixedItemHeight = 36f;
                // If your row heights vary, use dynamic height:
                // assetsPathFieldHolder.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            }

            SyncSettingsToUI();
        }

        

        protected LorekeeperSettingsFactory settingsFactory = new LorekeeperSettingsFactory();
        public LorekeeperSettings Settings { get; protected set; }

        protected ListView assetsPathFieldHolder;
        protected Button addPathButton, saveButton;

        protected Toggle trackAudioClipsToggle;
        protected Toggle trackAudioMixersToggle;
        protected Toggle trackSpritesToggle;
        protected Toggle trackTexturesToggle;
        protected Toggle trackRenderTexturesToggle;
        protected Toggle trackCubemapsToggle;
        protected Toggle trackMaterialsToggle;
        protected Toggle trackShadersToggle;
        protected Toggle trackComputeShadersToggle;
        protected Toggle trackAnimationClipsToggle;
        protected Toggle trackAnimatorControllersToggle;
        protected Toggle trackAvatarsToggle;
        protected Toggle trackModelsToggle;
        protected Toggle trackMeshesToggle;
        protected Toggle trackPrefabsToggle;
        protected Toggle trackFontsToggle;
        protected Toggle trackTmpFontAssetsToggle;
        protected Toggle trackScriptableObjectsToggle;
        protected Toggle trackTextAssetsToggle;
        protected Toggle trackPhysicsMaterialsToggle;
        protected Toggle trackPhysicsMaterials2DToggle;
        protected Toggle trackOtherToggle;

        protected virtual void ToggleSubs(bool on)
        {
            if (on)
            {
                assetsPathFieldHolder.makeItem = AssetPathFieldHolderMakeItem;
                assetsPathFieldHolder.bindItem += AssetPathFieldHolderBindItem;
                assetsPathFieldHolder.destroyItem += AssetPathFieldHolderDestroyItem;
                assetsPathFieldHolder.unbindItem += AssetPathFieldHolderUNbindItem;
                
                saveButton.clicked += OnSaveButtonClicked;
            }
            else
            {
                assetsPathFieldHolder.makeItem = null;
                assetsPathFieldHolder.bindItem -= AssetPathFieldHolderBindItem;
                assetsPathFieldHolder.destroyItem -= AssetPathFieldHolderDestroyItem;
                assetsPathFieldHolder.unbindItem -= AssetPathFieldHolderUNbindItem;

                saveButton.clicked -= OnSaveButtonClicked;
            }
        }

        protected VisualElement AssetPathFieldHolderMakeItem()
        {
            TextField elem = new TextField() { isDelayed = true };
            elem.style.height = 36f;
            elem.style.fontSize = 16;
            elem.SetValueWithoutNotify("/");
            elem.RegisterValueChangedCallback(OnTextFieldValueChanged);
            // ^Don't worry about unregistering this; the items get pooled by the ListView
            // When they're destroyed/unbound, the callback goes with them.
            return elem;
        }

        protected void OnTextFieldValueChanged(ChangeEvent<string> evt)
        {
            var field = (TextField)evt.target;
            var indexObj = field.userData;
            bool validIndex = indexObj is int i && i >= 0 && i < blacklistCopy.Count;
            if (validIndex)
            {
                i = (int)indexObj;
                blacklistCopy[i] = evt.newValue;
                Debug.Log($"[OnTextFieldValueChanged]: Text field at index {indexObj} changed to: {evt.newValue}");
            }
            else
            {
                Debug.LogWarning($"[OnTextFieldValueChanged]: Received invalid index {indexObj} for changed text field.");
            }
        }

        protected void AssetPathFieldHolderBindItem(VisualElement element, int index)
        {
            Debug.Log($"[AssetPathFieldHolderBindItem]: Binding item at index {index}");
            TextField tField = (TextField)element;
            tField.userData = index;
            tField.SetValueWithoutNotify(blacklistCopy[index]);
        }

        protected List<string> blacklistCopy = new List<string>();
        // ^A copy of the blacklist to work with in the UI. It will be used to update the actual settings on save.

        protected void AssetPathFieldHolderDestroyItem(VisualElement element)
        {
            AssetPathFieldHolderUNbindItem(element, -1);
        }

        protected void AssetPathFieldHolderUNbindItem(VisualElement element, int index)
        {
            TextField tField = (TextField)element;
            Debug.Log($"[AssetPathFieldHolderUNbindItem]: Unbinding item at index {index}: {tField.value}");
            tField.userData = null;
        }

        protected void OnSaveButtonClicked()
        {
            UIEvents.SaveButtonClicked();
            Settings.Blacklist = blacklistCopy;
            SyncUIToSettings();
            // ^To make sure it's updated with what's in the UI
            LKUtils.WriteSettingsToDisk(Settings);
        }

        protected virtual void OnEnable()
        {
            bool alreadyInitialized = assetsPathFieldHolder != null;
            if (alreadyInitialized)
            {
                Settings = settingsFactory.GetSettings();
                blacklistCopy.AddRange(Settings.Blacklist);
                ToggleSubs(true);
                assetsPathFieldHolder.itemsSource = blacklistCopy;
                assetsPathFieldHolder.Rebuild();
                SyncSettingsToUI();
            }
            // ^We need this since closing the window and then reopening it doesn't usually
            // get CreateGUI called twice
        }

        protected virtual void OnDisable()
        {
            ToggleSubs(false);
            Settings.Clear();
            blacklistCopy.Clear();
        }

        protected void SyncSettingsToUI()
        {
            trackAudioClipsToggle.SetValueWithoutNotify(Settings.TrackAudioClips);
            trackAudioMixersToggle.SetValueWithoutNotify(Settings.TrackAudioMixers);

            trackSpritesToggle.SetValueWithoutNotify(Settings.TrackSprites);
            trackTexturesToggle.SetValueWithoutNotify(Settings.TrackTextures);
            trackRenderTexturesToggle.SetValueWithoutNotify(Settings.TrackRenderTextures);
            trackCubemapsToggle.SetValueWithoutNotify(Settings.TrackCubemaps);
            trackMaterialsToggle.SetValueWithoutNotify(Settings.TrackMaterials);
            trackShadersToggle.SetValueWithoutNotify(Settings.TrackShaders);
            trackComputeShadersToggle.SetValueWithoutNotify(Settings.TrackComputeShaders);

            trackAnimationClipsToggle.SetValueWithoutNotify(Settings.TrackAnimationClips);
            trackAnimatorControllersToggle.SetValueWithoutNotify(Settings.TrackAnimatorControllers);
            trackAvatarsToggle.SetValueWithoutNotify(Settings.TrackAvatars);

            trackModelsToggle.SetValueWithoutNotify(Settings.TrackModels);
            trackMeshesToggle.SetValueWithoutNotify(Settings.TrackMeshes);
            trackPrefabsToggle.SetValueWithoutNotify(Settings.TrackPrefabs);

            trackFontsToggle.SetValueWithoutNotify(Settings.TrackFonts);
            trackTmpFontAssetsToggle.SetValueWithoutNotify(Settings.TrackTmpFontAssets);

            trackScriptableObjectsToggle.SetValueWithoutNotify(Settings.TrackScriptableObjects);
            trackTextAssetsToggle.SetValueWithoutNotify(Settings.TrackTextAssets);

            trackPhysicsMaterialsToggle.SetValueWithoutNotify(Settings.TrackPhysicsMaterials);
            trackPhysicsMaterials2DToggle.SetValueWithoutNotify(Settings.TrackPhysicsMaterials2D);

            trackOtherToggle.SetValueWithoutNotify(Settings.TrackOther);
        }

        protected void SyncUIToSettings()
        {
            Settings.TrackAudioClips = trackAudioClipsToggle.value;
            Settings.TrackAudioMixers = trackAudioMixersToggle.value;

            Settings.TrackSprites = trackSpritesToggle.value;
            Settings.TrackTextures = trackTexturesToggle.value;
            Settings.TrackRenderTextures = trackRenderTexturesToggle.value;
            Settings.TrackCubemaps = trackCubemapsToggle.value;
            Settings.TrackMaterials = trackMaterialsToggle.value;
            Settings.TrackShaders = trackShadersToggle.value;
            Settings.TrackComputeShaders = trackComputeShadersToggle.value;

            Settings.TrackAnimationClips = trackAnimationClipsToggle.value;
            Settings.TrackAnimatorControllers = trackAnimatorControllersToggle.value;
            Settings.TrackAvatars = trackAvatarsToggle.value;

            Settings.TrackModels = trackModelsToggle.value;
            Settings.TrackMeshes = trackMeshesToggle.value;
            Settings.TrackPrefabs = trackPrefabsToggle.value;

            Settings.TrackFonts = trackFontsToggle.value;
            Settings.TrackTmpFontAssets = trackTmpFontAssetsToggle.value;

            Settings.TrackScriptableObjects = trackScriptableObjectsToggle.value;
            Settings.TrackTextAssets = trackTextAssetsToggle.value;

            Settings.TrackPhysicsMaterials = trackPhysicsMaterialsToggle.value;
            Settings.TrackPhysicsMaterials2D = trackPhysicsMaterials2DToggle.value;

            Settings.TrackOther = trackOtherToggle.value;
        }
    }

}