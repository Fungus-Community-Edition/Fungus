using UnityEngine;

namespace Amanita
{
    /// <summary>
    /// Global constants used in various parts of Amanita.
    /// </summary>
    public static class AmanitaConstants
    {
        /// <summary>
        /// Duration of fade for executing icon displayed beside blocks & commands.
        /// </summary>
        public const float ExecutingIconFadeTime = 0.5f;

        /// <summary>
        /// The current version of the Flowchart. Used for updating components.
        /// </summary>
        public const int CurrentVersion = 1;

        /// <summary>
        /// The name of the initial block in a new flowchart.
        /// </summary>
        public const string DefaultBlockName = "New Block";

        /// <summary>
        /// The default choice block color.
        /// </summary>
        public static Color DefaultChoiceBlockTint = new Color(1.0f, 0.627f, 0.313f, 1.0f);

        /// <summary>
        /// The default event block color.
        /// </summary>
        public static Color DefaultEventBlockTint = new Color(0.784f, 0.882f, 1.0f, 1.0f);

        /// <summary>
        /// The default process block color.
        /// </summary>
        public static Color DefaultProcessBlockTint = new Color(1.0f, 0.882f, 0.0f, 1.0f);

        /// <summary>
        /// The default key used for storing save game data in PlayerPrefs.
        /// </summary>
        public const string DefaultSaveDataKey = "save_data";

        public const string FungusAudioMixer = "FungusAudioMixer";

        public const string UIPrefixForDeprecated = "[DEP] ";
        public const string UIPrefixForDeprecated_RichText = "<color=yellow>" + UIPrefixForDeprecated + "</color>";

        public const string PathToAmanitaManagerPrefab = "Prefabs/AmanitaManager";
        public const string PathToDefaultTweenAdapter = "DefaultTweenAdapter";
        public const string PathToDefaultSaveStorageSettings = "SaveSys/Defaults/DefaultSaveStorageSettings";
        public const string PathToDefaultEncryptor = "SaveSys/Defaults/DefaultEncryptor";
        public const string PathToSaveSysDefaultsFolder = "SaveSys/Defaults";
        public const string PathToDefaultDecryptor = "SaveSys/Defaults/DefaultDecryptor";
        public const string PathToDefaultSaveReader = "SaveSys/Defaults/DefaultSaveReader";
        public const string PathToDefaultSaveWriter = "SaveSys/Defaults/DefaultSaveWriter";

        /// <summary>
        /// This is relative to a Resources folder.
        /// </summary>
        public const string PathToAmanitaVariableDisplayEditorUxml = "UIToolkitTemplates/VariableDisplayEditor";

        /// <summary>
        /// The default name of the Input EventSystem, stored in the resources folder.
        /// </summary>
        public const string EventSystemPrefabName =
#if ENABLE_INPUT_SYSTEM
            "Prefabs/EventSystem_NewInputSystem";
#else
            "Prefabs/EventSystem";
#endif

        public const string SaveNameVarName = "saveName",
            SaveNamePrefixVarName = "saveNamePrefix",
            SaveNameSuffixVarName = "saveNameSuffix";
    }
}
