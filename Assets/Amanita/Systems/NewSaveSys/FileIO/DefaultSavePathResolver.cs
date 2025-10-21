using UnityEngine;
using System.IO;

namespace Amanita.SaveSys
{
    public class DefaultSavePathResolver : IConfigurableSaveSlotPathResolver,
        ISavePathResolver<SaveDirectoryType>,
        ISaveSlotPathResolver<SaveDirectoryType>
    {
        public DefaultSavePathResolver(string relativePath = "Saves", string fileExtension = "save")
        {
            // Given Unity's quirks, we have our own separate relative path and file extension
            // fields so that we work fine even with no StorageSettings assigned
            this.relativePath = relativePath;
            this.fileExtension = fileExtension;
        }

        public virtual string RelativePath
        {
            get
            {
                if (StorageSettings == null)
                {
                    return relativePath;
                }
                return StorageSettings.RelativePath;
            }
            set
            {
                relativePath = value;

                if (StorageSettings != null)
                {
                    StorageSettings.RelativePath = value;
                }
            }
        }

        protected string relativePath = "Saves";
        protected static readonly string defaultRelativePath = "Saves";

        public string FileExtension
        {
            get
            {
                if (StorageSettings == null)
                {
                    return fileExtension;
                }
                return StorageSettings.FileExtension;
            }
            set
            {
                var processedExtension = value?.Trim().TrimStart('.');

                if (string.IsNullOrEmpty(processedExtension))
                {
                    Debug.LogWarning($"File extension is empty or null after trimming. Reverting " +
                        $"it to the default: {defaultFileExtension}");
                    fileExtension = defaultFileExtension;
                }
                else
                {
                    fileExtension = processedExtension;
                }

                if (StorageSettings != null)
                {
                    StorageSettings.FileExtension = fileExtension;
                }
            }
        }
        protected string fileExtension = "save";
        protected static readonly string defaultFileExtension = "save";

        public string GetSaveFilePath(string fileName, object input)
        {
            if (input is not SaveDirectoryType)
            {
                Debug.LogWarning("Input is not of type SaveDirectoryType. Acting as if it was " +
                    "SaveDirectoryType.PersistentDataPath.");
                input = SaveDirectoryType.PersistentDataPath;
            }

            return GetSaveFilePath(fileName, (SaveDirectoryType)input);
        }

        public string GetSaveFilePath(string fileName, SaveDirectoryType input)
        {
            string folderPath = GetSaveFolderPath(input);
            fileName = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithExt = $"{fileName}.{FileExtension}";
            string result = Path.Join(folderPath, fileNameWithExt);
            return result;
        }

        public string GetSaveFolderPath(object input)
        {
            string result;
            if (input is not SaveDirectoryType)
            {
                Debug.LogWarning("Input is not of type SaveDirectoryType. Acting as if it was " +
                    "SaveDirectoryType.PersistentDataPath.");
                input = SaveDirectoryType.PersistentDataPath;
            }

            result = GetSaveFolderPath((SaveDirectoryType)input);
            return result;
        }

        public string GetSaveFolderPath(SaveDirectoryType pathEnum)
        {
            string basePath = GetBasePath();
            string GetBasePath()
            {
                switch (pathEnum)
                {
                    case SaveDirectoryType.PersistentDataPath:
                        return Application.persistentDataPath;
                    case SaveDirectoryType.DataPath:
                    case SaveDirectoryType.InTheBalls: // Heh, nice.
                        return Application.dataPath;
                    default:
                        Debug.LogWarning("Unrecognized SaveDirectoryType. Defaulting to empty path.");
                        return string.Empty;
                }
            }

            string result = string.Empty;
            if (!string.IsNullOrEmpty(basePath))
            {
                // We don't want to add a relative path if it's empty or null.
                // We want the clients to know that the input caused an error.
                result = Path.Join(basePath, RelativePath);
            }
            return result;
        }

        public string GetSaveFilePath(SaveDirectoryType input, int slotNumber)
        {
            string folderPath = GetSaveFolderPath(input);
            string fileName = GetSaveFileName(slotNumber);
            string result = Path.Join(folderPath, fileName);
            return result;
        }

        public string GetSaveFileName(int slotNumber)
        {
            // Can't simply take the path and then have Path.IO strip things;
            // we need to decide the name with the slot number in mind.
            string fileNumFormatted = slotNumber.ToString(SlotNumberFormat);
            string result = string.Format(FileNameFormat, Prefix,
                fileNumFormatted, FileExtension);

            return result;
        }

        protected virtual string SlotNumberFormat
        {
            get
            {
                if (StorageSettings == null)
                {
                    return "D2";
                }

                return StorageSettings.SlotNumberFormat;
            }
        }

        protected virtual string FileNameFormat
        {
            get
            {                 
                if (StorageSettings == null)
                {
                    return "{0}_{1}.{2}";
                }
                return StorageSettings.FileNameFormat;
            }
        }

        protected virtual string Prefix
        {
            get
            {
                if (StorageSettings == null)
                {
                    return "saveData";
                }
                return StorageSettings.Prefix;
            }
        }

        public string GetSaveFilePath(object input, int slotNumber)
        {
            if (input is not SaveDirectoryType)
            {
                Debug.LogWarning("Input is not of type SaveDirectoryType. Acting as if it was " +
                    "SaveDirectoryType.PersistentDataPath.");
                input = SaveDirectoryType.PersistentDataPath;
            }

            return GetSaveFilePath((SaveDirectoryType)input, slotNumber);
        }

        public SaveStorageSettings StorageSettings
        {
            get
            {
                if (storageSettings == null)
                {
                    storageSettings = DefaultAmanitaAssets.SaveStorageSettings;
                }

                return storageSettings;
            }
            set
            {
                storageSettings = value;
                if (storageSettings == null)
                {
                    Debug.LogWarning($"[DefaultSavePathResolver]: Assigned StorageSettings is null. " +
                        $"Reverting to default StorageSettings asset.");
                    storageSettings = DefaultAmanitaAssets.SaveStorageSettings;
                }
                fileExtension = storageSettings.FileExtension;
                relativePath = storageSettings.RelativePath;
            }
        }

        protected SaveStorageSettings storageSettings;

        public string NumberFormat
        {
            get
            {
                return storageSettings.SlotNumberFormat;
            }
            set
            {
                // We only want to change the format if we're working with a non-default storage settings asset
                if (ReferenceEquals(storageSettings, DefaultAmanitaAssets.SaveStorageSettings))
                {
                    Debug.LogWarning($"[DefaultSavePathResolver]: Attempted to change the slot number format when this is " +
                        $"using the default storage settings. This is not allowed. Assign this a different Storage Settings SO " +
                        $"to use a different slot number format.");
                    return;
                }

                storageSettings.SlotNumberFormat = value;
            }
        }
    }
}