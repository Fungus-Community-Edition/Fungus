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
            string saveFolderPath = GetAndPrepSaveFolderPath(request);
            GetFullFilePath(request, saveFolderPath, out string fullFilePath);
            
            ISaveMetaData result = null;
            string wholeText = File.ReadAllText(fullFilePath);
            // We assume that the metadata and main data are written as separate strings
            if (!readEncrypted)
            {
                IList<string> splitIntoJsons = wholeText.Split(new string[] { ReadWriteDelimiter }, StringSplitOptions.None);
                string jsonForMetadata = splitIntoJsons[0];
                // We don't care about the main data in this func, so we'll ignore it
                result = JsonUtility.FromJson<SaveMetaData>(jsonForMetadata);
            }
            else
            {
                result = usableDecryptor.DecryptMeta(wholeText);
            }

            return result;
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

        protected virtual void GetFullFilePath(SaveReadRequest request, string saveFolderPath, out string filePath)
        {
            string fileName = string.Format(fileNameFormat, savePrefix, request.SlotNumber, fileExtension);
            filePath = string.Format(filePathFormat, saveFolderPath, fileName);
        }

        public virtual CompositeSaveData ReadMainSaveDataFromDisk(SaveReadRequest request)
        {
            string saveFolderPath = GetAndPrepSaveFolderPath(request);
            GetFullFilePath(request, saveFolderPath, out string filePath);

            bool writtenAsPlainText = !readEncrypted;

            byte[] rawBytes = File.ReadAllBytes(filePath);
            object[] infoForDecryptor = new object[] { rawBytes, writtenAsPlainText };

            CompositeSaveData result = (CompositeSaveData) usableDecryptor.DecryptMainState(infoForDecryptor);
            
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
        public SaveReadRequest() { }
    }
}