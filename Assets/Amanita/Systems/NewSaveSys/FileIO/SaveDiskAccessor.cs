using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject
    {
        [SerializeField] protected SaveNameSettings nameSettings;

        #region Name Settings
        public virtual SaveNameSettings NameSettings
        {
            get => nameSettings;
            set
            {
                if (value == null)
                {
                    throw new System.ArgumentNullException(nameof(value), "Name settings cannot be null.");
                }
                nameSettings = value;
            }
        }
        public virtual string SavePrefix
        {
            get => nameSettings.Prefix;
            set => nameSettings.Prefix = value;
        }

        public virtual string FileExtension => nameSettings.FileExtension;

        public virtual string RelativeSavePath
        {
            get => nameSettings.RelativeSavePath;
            set => nameSettings.RelativeSavePath = value;
        }

        public virtual string SaveNumberFormat => nameSettings.NumberFormat;

        public virtual string DefaultRelativeSavePath => nameSettings.DefaultRelativeSavePath;

        public virtual string FileNameFormat => nameSettings.FileNameFormat;
        public virtual string FilePathFormat => nameSettings.FilePathFormat;
        #endregion

        public static string ReadWriteDelimiter => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

        protected virtual string GetFolderToAccess(SaveDirectoryType directoryType)
        {
            string result = FileUtils.GetPathToFolder(directoryType, RelativeSavePath);
            return result;
        }

        // For checking the validity of the save files.
        public static string CompletionMarker { get; protected set; } = "\n<!-- Amanita Save Sys: Save Completed! -->";
        
        protected virtual void OnEnable()
        {
            EnsureWeHaveNameSettings();
        }

        protected virtual void EnsureWeHaveNameSettings()
        {
            if (nameSettings == null)
            {
                string pathToDefaultSettings = "SaveSys/DefaultSaveNameSettings"; // Relative to the Resources folder
                nameSettings = Resources.Load<SaveNameSettings>(pathToDefaultSettings);

                if (nameSettings == null)
                {
                    // Create a default SaveNameSettings instance, putting it in Resources afterward
                    nameSettings = CreateInstance<SaveNameSettings>();
                }

#if UNITY_EDITOR
                string assetPath = "Assets/Amanita/Resources/SaveSys/DefaultSaveNameSettings.asset";
                UnityEditor.AssetDatabase.CreateAsset(nameSettings, assetPath);
                UnityEditor.AssetDatabase.SaveAssets();
#endif

            }
        }

        protected virtual void OnValidate()
        {
            EnsureWeHaveNameSettings();
        }

       
    }
}