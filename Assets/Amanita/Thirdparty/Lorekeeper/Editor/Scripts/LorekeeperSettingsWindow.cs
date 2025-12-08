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
            window.maxSize = window.minSize = windowSize;
            window.titleContent = new GUIContent("LorekeeperSettings");
        }

        protected static Vector2 windowSize = new Vector2(600, 400);
        
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

        }

        

        protected LorekeeperSettingsFactory settingsFactory = new LorekeeperSettingsFactory();
        public LorekeeperSettings Settings { get; protected set; }

        protected ListView assetsPathFieldHolder;
        protected Button addPathButton, saveButton;

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
    }

}