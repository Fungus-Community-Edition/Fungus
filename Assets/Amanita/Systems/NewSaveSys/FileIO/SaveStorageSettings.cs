using UnityEngine;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "NewSaveStorageSettings", menuName = "Amanita/SaveSys/SaveStorageSettings")]
    /// <summary>
    /// Settings for how save files are named and stored in directories.
    /// </summary>
    public class SaveStorageSettings : ScriptableObject
    {
        [Header("File Naming")]
        [Tooltip("The first part of the save files' names this works with.")]
        [SerializeField] protected string prefix = "saveData";
        [Tooltip("The format for the number part of the save files' names this works with. " +
            "D2 means it always has 2 digits.")]
        [SerializeField] protected string numberFormat = "D2";
        [Tooltip("Just for flavor.")]
        [SerializeField] protected string fileExtension = "save";

        [Header("File Location")]
        [Tooltip("Root directory for the save files.")]
        [SerializeField] protected SaveDirectoryType directoryType = SaveDirectoryType.DataPath;
        [SerializeField] protected string relativePath = "/Saves/";

        [Header("Security")]
        [Tooltip("Note that Amanita's default encryption does not guarantee security, " +
            "just basic obfuscation.")]
        [SerializeField] protected bool expectEncryption = false;

        public virtual string Prefix
        {
            get => prefix;
            set => prefix = value;
        }
        public virtual string FileExtension
        {
            get => fileExtension;
            set
            {
                var processedExtension = value?.Trim().TrimStart('.');
                if (string.IsNullOrEmpty(processedExtension))
                {
                    Debug.LogWarning($"File extension is empty or null after trimming. Reverting " +
                        $"it to the default: {DefaultFileExtension}");
                    fileExtension = DefaultFileExtension;
                }
                else
                {
                    fileExtension = processedExtension;
                }
            }
        }

        public static string DefaultFileExtension => "save";

        public virtual string RelativePath
        {
            get => relativePath;
            set
            {
                relativePath = value;
                if (string.IsNullOrEmpty(relativePath))
                {
                    relativePath = "/";
                }
                EnsureRelativePathInRightFormat();
            }
        }

        protected virtual void EnsureRelativePathInRightFormat()
        {
            bool isJustDash = relativePath == "/" || relativePath == "\\";
            if (isJustDash)
            {
                relativePath = DefaultRelativeSavePath;
            }
            bool startsWithDash = relativePath.StartsWith('/') || relativePath.StartsWith("\\");
            if (startsWithDash)
            {
                relativePath = relativePath.TrimStart('/', '\\');
            }
        }

        public virtual string DefaultRelativeSavePath => "Saves";

        /// <summary>
        /// Note that Amanita's default encryption
        /// does not guarantee security, just basic obfuscation.
        /// </summary>
        public virtual bool ExpectEncryption
        {
            get => expectEncryption;
            set => expectEncryption = value;
        }

        public virtual string SlotNumberFormat
        {
            get => numberFormat;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    Debug.LogWarning($"Provided number format was null or empty. " +
                        $"Reverting to default: '{DefaultNumberFormat}'");
                    numberFormat = DefaultNumberFormat;
                }
                else
                {
                    numberFormat = value;
                }
            }
        }

        public static string DefaultNumberFormat => "D2";

        public virtual SaveDirectoryType DirectoryType
        {
            get => directoryType;
            set
            {
                // We want to keep in mind the platform we're running on. Some don't allow us to 
                // use DataPath.
                directoryType = value;
                OverrideChoiceIfNeeded();
                void OverrideChoiceIfNeeded()
                {
                    RuntimePlatform platform = Application.platform;
                    switch (directoryType)
                    {
                        case SaveDirectoryType.DataPath:
                        case SaveDirectoryType.InTheBalls:
                            if (platform == RuntimePlatform.WebGLPlayer ||
                                platform == RuntimePlatform.tvOS ||
                                platform == RuntimePlatform.Android ||
                                platform == RuntimePlatform.IPhonePlayer)
                            {
                                Debug.LogWarning($"DataPath is not supported on {platform}. " +
                                    $"Reverting to PersistentDataPath.");
                                directoryType = SaveDirectoryType.PersistentDataPath;
                            }
                            break;
                        default:
                            Debug.LogWarning($"DirectoryType {directoryType} is not recognized. " +
                                $"Reverting to PersistentDataPath.");
                            directoryType = SaveDirectoryType.PersistentDataPath; break;
                    }
                }
            }
        }

        protected string fileNameFormat = "{0}_{1}.{2}";
        protected string filePathFormat = "{0}{1}"; // We expect a / or \ at the end of {0}
        public virtual string FileNameFormat => fileNameFormat;
        public virtual string FilePathFormat => filePathFormat;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(RelativePath))
            {
                RelativePath = DefaultRelativeSavePath;
            }

            EnsureRelativePathInRightFormat();

            if (string.IsNullOrEmpty(Prefix))
            {
                Prefix = DefaultSavePrefix;
            }
        }

        public static string DefaultSavePrefix => "saveData";

    }
}
