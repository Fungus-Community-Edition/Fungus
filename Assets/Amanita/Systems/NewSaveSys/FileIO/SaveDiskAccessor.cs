using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject, ISaveFolderPathResolver,
        ISaveFilePathResolver, ISaveSlotPathResolver, IHasConfigurableSaveSlotPathResolver
    {
        [SerializeField] protected SaveStorageSettings storageSettings;

        /// <summary>
        /// Decides what paths this accessor would use for reading and/or writing.
        /// </summary>
        public virtual IConfigurableSaveSlotPathResolver PathResolver
        {
            get => pathResolver;
            set
            {
                if (value == null)
                {
                    throw new System.ArgumentNullException(nameof(value), "Path resolver cannot be null.");
                }
                pathResolver = value;
            }
        }
        protected IConfigurableSaveSlotPathResolver pathResolver = new DefaultSavePathResolver();

        #region Storage Settings
        public virtual SaveStorageSettings StorageSettings
        {
            get => storageSettings;
            set
            {
                if (value == null)
                {
                    throw new System.ArgumentNullException(nameof(value), "Name settings cannot be null.");
                }
                storageSettings = value;
            }
        }
        public virtual string SavePrefix
        {
            get => storageSettings.Prefix;
            set => storageSettings.Prefix = value;
        }

        public virtual string FileExtension => storageSettings.FileExtension;

        public virtual string RelativeSavePath
        {
            get => storageSettings.RelativePath;
            set => storageSettings.RelativePath = value;
        }

        public virtual string SaveNumberFormat => storageSettings.SlotNumberFormat;

        public virtual string DefaultRelativeSavePath => storageSettings.DefaultRelativeSavePath;

        public virtual string FileNameFormat => storageSettings.FileNameFormat;
        public virtual string FilePathFormat => storageSettings.FilePathFormat;
        #endregion

        public static readonly string[] ReadWriteDelimiters = new string[]
        {
            $"\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n", 
            // ^The basic one, with just \n instances. This is what we will write to the file, and what
            // we will expect to see when reading.

            $"\r\n\r\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\r\n\r\n",
            // ^But sometimes, some platforms (looking at you, WebGL) don't
            // handle \n the same as others. That, and the save writer might append \r instances
            // for whatever reason, so we want to be able to handle those as well.
        };

        public virtual bool ExpectEncryption
        {
            get => storageSettings.ExpectEncryption;
            set => storageSettings.ExpectEncryption = value;
        }

        protected virtual string GetFolderToAccess(SaveDirectoryType directoryType)
        {
            string result = pathResolver.GetSaveFolderPath(directoryType);
            return result;
        }

        // For checking the validity of the save files.
        public static string CompletionMarker { get; protected set; } = "\n<!-- Amanita Save Sys: Save Completed! -->";

        public string RelativePath => storageSettings.RelativePath;

        public string NumberFormat => ((ISaveSlotPathResolver)pathResolver).NumberFormat;

        protected virtual void OnEnable()
        {
            EnsureWeHaveStorageSettings();
            SyncResolverToSettings();
        }

        protected virtual void EnsureWeHaveStorageSettings()
        {
            if (storageSettings == null)
            {
                storageSettings = DefaultAmanitaAssets.SaveStorageSettings;
                if (storageSettings == null)
                {
                    Debug.LogWarning("No SaveStorageSettings assigned to SaveDiskAccessor, and no default found. Creating a new instance.");
                    storageSettings = CreateInstance<SaveStorageSettings>();
                }
                
            }
        }

        protected virtual void SyncResolverToSettings()
        {
            pathResolver.RelativePath = storageSettings.RelativePath;
            pathResolver.FileExtension = storageSettings.FileExtension;
        }

        protected virtual void OnValidate()
        {
            EnsureWeHaveStorageSettings();
            SyncResolverToSettings();
        }

        public string GetSaveFolderPath(object input)
        {
            return PathResolver.GetSaveFolderPath(input);
        }

        public string GetSaveFilePath(string fileName, object input)
        {
            return pathResolver.GetSaveFilePath(fileName, input);
        }

        public string GetSaveFilePath(object input, int slotNumber)
        {
            return pathResolver.GetSaveFilePath(input, slotNumber);
        }

        public string GetSaveFileName(int slotNumber)
        {
            return pathResolver.GetSaveFileName(slotNumber);
        }
    }
}