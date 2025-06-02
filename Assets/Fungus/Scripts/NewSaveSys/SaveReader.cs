using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "NewSaveReader", menuName = "Amanita/SaveSys/SaveReader")]
    public class SaveReader : SaveDiskAccessor
    {
        [SerializeField] protected bool readEncrypted = false;
        public virtual bool ReadEncrypted
        {
            get => readEncrypted;
            set => readEncrypted = value;
        }

        [SerializeField] protected ScriptableObject decryptor;

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        protected virtual void OnEnable()
        {
            PrepDefaultDecryptor();
            void PrepDefaultDecryptor()
            {
                if (defaultDecryptor == null)
                {
                    defaultDecryptor = CreateInstance<Decryptor>();
                }
            }

            if (decryptor == null)
            {
                decryptor = defaultDecryptor;
            }

            usableDecryptor = decryptor as IDecryptor;
        }

        protected Decryptor defaultDecryptor;
        protected IDecryptor usableDecryptor;

        public virtual ISaveMetaData ReadMetadataFromDisk(SaveReadRequest request)
        {
            string filePath = GetFullFilePath(request);
            Validate(filePath);

            bool writtenAsPlainText = !readEncrypted;
            byte[] rawBytes = File.ReadAllBytes(filePath);
            object[] infoForDecryptor = new object[] { rawBytes, writtenAsPlainText };

            SaveMetaData result = (SaveMetaData)usableDecryptor.DecryptMeta(infoForDecryptor);
            return result;
        }

        protected virtual string GetFullFilePath(SaveReadRequest request)
        {
            string saveFolderPath = GetAndPrepSaveFolderPath(request);
            string fileName = string.Format(fileNameFormat, savePrefix,
                request.SlotNumber.ToString("D3"), fileExtension);
            string filePath = string.Format(filePathFormat, saveFolderPath, fileName);
            return filePath;
        }

        protected virtual string GetAndPrepSaveFolderPath(SaveReadRequest request)
        {
            string saveFolder = SaveSystem.SaveDirectoryPaths[request.BaseSaveDirectory];
            bool thereIsRelativePathToConsider = relativeSavePath.Count() > 0;
            if (thereIsRelativePathToConsider)
            {
                saveFolder = Path.Combine(saveFolder, relativeSavePath);
            }

            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.
            return saveFolder;
        }

        protected virtual void Validate(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                string errorMessage = $"Cannot read metadata of file {fileName}, because it is just like Santa Claus: it doesn't exist";
                throw new FileNotFoundException(errorMessage);
            }
        }

        public virtual CompositeSaveData ReadMainSaveDataFromDisk(SaveReadRequest request)
        {
            string filePath = GetFullFilePath(request);
            Validate(filePath);

            bool writtenAsPlainText = !readEncrypted;

            byte[] rawBytes = File.ReadAllBytes(filePath);
            object[] infoForDecryptor = new object[] { rawBytes, writtenAsPlainText };

            CompositeSaveData result = (CompositeSaveData) usableDecryptor.DecryptMainState(infoForDecryptor);
            
            return result;
        }

        public virtual string GetSavePath(SaveReadRequest request)
        {
            string result = GetFullFilePath(request);
            return result;
        }

        protected virtual void OnValidate()
        {
            bool wrongTypeOfSOAssigned = decryptor != null && decryptor is not IDecryptor;
            if (wrongTypeOfSOAssigned)
            {
                decryptor = defaultDecryptor;
                Debug.LogError($"Tried to assign a Scriptable Object that does not implement IDecryptor. Reverting to default.");
            }
        }
    }

    public class SaveReadRequest : EventArgs
    {
        public virtual int SlotNumber { get; set; } = 0;
        public virtual SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveReadRequest()
        {

        }

        public SaveReadRequest(SaveReadRequest other)
        {
            this.SlotNumber = other.SlotNumber;
            BaseSaveDirectory = other.BaseSaveDirectory;
        }
    }
}