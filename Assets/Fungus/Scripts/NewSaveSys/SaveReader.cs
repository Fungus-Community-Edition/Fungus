using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;

namespace Amanita.SaveSys
{
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
            string saveFolder, fileName, filePath;

            GetAndPrepSaveFolderPath();
            void GetAndPrepSaveFolderPath()
            {
                saveFolder = SaveSystem.SaveDirectoryPaths[request.BaseSaveDirectory];
                bool thereIsRelativePathToConsider = relativeSavePath.Count() > 0;
                if (thereIsRelativePathToConsider)
                {
                    saveFolder = Path.Combine(saveFolder, relativeSavePath);
                }

                Directory.CreateDirectory(saveFolder); // In case it doesn't exist.
            }

            GetFileNameAndPath();
            void GetFileNameAndPath()
            {
                fileName = string.Format(fileNameFormat, savePrefix, request.SlotNumber, fileExtension);
                filePath = string.Format(filePathFormat, saveFolder, fileName);
            }

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
    
        public virtual AmanitaSaveData ReadMainSaveDataFromDisk(SaveReadRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class SaveReadRequest : EventArgs
    {
        public virtual int SlotNumber { get; set; } = 0;
        public virtual SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveReadRequest() { }
    }
}