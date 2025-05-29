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

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        public virtual SaveMetaData ReadMetadataFromDisk(SaveReadRequest request)
        {
            string saveFolderPath = GetAndPrepSaveFolderPath(request);
            GetFileNameAndPath(request, saveFolderPath, out string filePath);
            
            SaveMetaData result = null;
            // We assume that the metadata and main data are written as separate strings
            if (!readEncrypted)
            {
                string wholeText = File.ReadAllText(filePath);
                IList<string> splitIntoJsons = wholeText.Split(new string[] { ReadWriteDelimiter }, StringSplitOptions.None);
                string jsonForMetadata = splitIntoJsons[0];
                // We don't care about the main data in this func, so we'll ignore it
                result = JsonUtility.FromJson<SaveMetaData>(jsonForMetadata);
            }
            else
            {
                throw new NotImplementedException("Didn't implement reading encrypted data yet.");
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

        protected virtual void GetFileNameAndPath(SaveReadRequest request, string saveFolderPath, out string filePath)
        {
            string fileName = string.Format(fileNameFormat, savePrefix, request.SlotNumber, fileExtension);
            filePath = string.Format(filePathFormat, saveFolderPath, fileName);
        }

        public virtual AmanitaSaveData ReadMainSaveDataFromDisk(SaveReadRequest request)
        {
            string saveFolderPath = GetAndPrepSaveFolderPath(request);
            GetFileNameAndPath(request, saveFolderPath, out string filePath);

            AmanitaSaveData result = null;
            // We assume that the metadata and main data are written as separate strings
            if (!readEncrypted)
            {
                result = ReadRaw();
                AmanitaSaveData ReadRaw()
                {
                    AmanitaSaveData result;
                    string wholeText = File.ReadAllText(filePath);
                    IList<string> splitIntoJsons = wholeText.Split(new string[] { ReadWriteDelimiter }, StringSplitOptions.None);
                    string jsonForMainSaveData = splitIntoJsons[1];
                    result = JsonUtility.FromJson<AmanitaSaveData>(jsonForMainSaveData);
                    return result;
                }
            }
            else
            {
                AmanitaSaveData ReadEncrypted()
                {
                    AmanitaSaveData result;
                    string wholeText = File.ReadAllText(filePath);
                    // Expected to be a byte array encoded by the default save writer


                    throw new NotImplementedException();
                }
                throw new NotImplementedException("Didn't implement reading encrypted data yet.");
            }

            return result;
        }
    }

    public class SaveReadRequest : EventArgs
    {
        public virtual int SlotNumber { get; set; } = 0;
        public virtual SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveReadRequest() { }
    }
}