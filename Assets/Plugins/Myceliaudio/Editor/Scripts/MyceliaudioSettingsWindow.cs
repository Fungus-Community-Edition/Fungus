using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

namespace AtMycelia.Myceliaudio.Editor
{
    public class MyceliaudioSettingsWindow : EditorWindow
    {
        [SerializeField]
        protected VisualTreeAsset m_VisualTreeAsset = default;

        [SerializeField]
        protected StyleSheet m_StyleSheet = default;

        [MenuItem("Tools/Amanita/Myceliaudio Settings")]
        public static void ShowExample()
        {
            MyceliaudioSettingsWindow window = GetWindow<MyceliaudioSettingsWindow>();
            window.titleContent = new GUIContent("Myceliaudio Settings");
            window.minSize = MinSize;
            window.maxSize = MinSize;
        }

        protected static Vector2 MinSize { get; set; } = new Vector2(700, 500);

        public virtual void CreateGUI()
        {
            container = rootVisualElement;
            PrepSettings();
            LoadVisualTree();
            LoadStyleSheet();
            PrepControls();
        }

        protected VisualElement container;

        protected virtual void PrepSettings()
        {
            settingsFilePath = Path.Combine(Application.dataPath, AudioSystem.SystemSettingsFileName);

            if (File.Exists(settingsFilePath))
            {
                string jsonText = File.ReadAllText(settingsFilePath);
                MyceliaudioSettings = JsonUtility.FromJson<MyceliaudioSettings>(jsonText);
            }
            else
            {
                MyceliaudioSettings = new MyceliaudioSettings();
                SaveSettingsToFile();
            }
        }

        protected MyceliaudioSettings MyceliaudioSettings { get; private set; }

        protected string settingsFilePath;

        protected virtual void SaveSettingsToFile()
        {
            string whatToWrite = JsonUtility.ToJson(MyceliaudioSettings, true);
            File.WriteAllText(settingsFilePath, whatToWrite);
        }

        protected virtual void LoadVisualTree()
        {
            string treeName = "MyceliaudioSettingsWindow.uxml";
            string pathToVisualTree = Path.Combine(Paths.ToEditorWindow, treeName);
            // ^VisualTrees are the uxml files containing the layout and such as you set it
            // up in UI Builder. Of course, as we're loading style sheets separately, 
            // visual trees themselves don't have style sheets; they have references to such
            container.Add(m_VisualTreeAsset.Instantiate());
        }

        protected virtual void LoadStyleSheet()
        {
            container.styleSheets.Add(m_StyleSheet);
        }

        protected virtual StyleSheet StyleSheetAt(string pathToSheet)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(pathToSheet);
        }

        protected virtual VisualTreeAsset VisualTreeAt(string pathToTree)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(pathToTree);
        }

        protected virtual void PrepControls()
        {
            PrepSliders();

            string controlName = "SaveButton";
            saveButton = container.Q<Button>(controlName);
            saveButton.clicked += OnSaveButtonClicked;
        }

        protected virtual void PrepSliders()
        {
            string controlName = "MasterVolumeSlider";
            masterVolSlider = container.Q<SliderInt>(controlName);

            controlName = "BGMusicSlider";
            bgMusicVolSlider = container.Q<SliderInt>(controlName);

            controlName = "SoundFXSlider";
            soundFxVolSlider = container.Q<SliderInt>(controlName);

            controlName = "VoiceSlider";
            voiceVolSlider = container.Q<SliderInt>(controlName);

            masterVolSlider.RegisterValueChangedCallback(OnSliderVolChanged);
            bgMusicVolSlider.RegisterValueChangedCallback(OnSliderVolChanged);
            soundFxVolSlider.RegisterValueChangedCallback(OnSliderVolChanged);
            voiceVolSlider.RegisterValueChangedCallback(OnSliderVolChanged);

            var volume = MyceliaudioSettings.Volume;
            float initMasterVol = volume.master, initBgMusicVol = volume.bgMusic,
                initSoundFxVol = volume.soundFX, initVoiceVol = volume.voice;

            masterVolSlider.value = (int)initMasterVol;
            bgMusicVolSlider.value = (int)initBgMusicVol;
            soundFxVolSlider.value = (int)initSoundFxVol;
            voiceVolSlider.value = (int)initVoiceVol;
        }

        private void OnSliderVolChanged(ChangeEvent<int> evt)
        {
            // We assume that the slider is parented to the same thing that the
            // value display is parented to
            SliderInt slider = evt.currentTarget as SliderInt;

            Label valueLabel = slider.parent.Q<Label>(valueLabelName);
            valueLabel.text = $"{slider.value}%";

            UpdateSettings();
        }

        protected string valueLabelName = "ValueDisplay";

        protected SliderInt masterVolSlider;

        protected virtual void UpdateSettings()
        {
            var volume = MyceliaudioSettings.Volume;
            volume.master = masterVolSlider.value;
            volume.bgMusic = bgMusicVolSlider.value;
            volume.soundFX = soundFxVolSlider.value;
            volume.voice = voiceVolSlider.value;
        }
        
        protected SliderInt bgMusicVolSlider, soundFxVolSlider, voiceVolSlider;

        protected Button saveButton;

        protected virtual void OnSaveButtonClicked()
        {
            SaveSettingsToFile();
        }

    }
}
