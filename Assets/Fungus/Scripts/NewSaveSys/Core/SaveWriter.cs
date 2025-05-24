using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for writing save data to disk.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSaveWriter", menuName = "Amanita/SaveSys/SaveWriter")]
    public class SaveWriter : SaveDiskAccessor
    {
        [Tooltip("Does not yet work.")]
        [SerializeField] protected bool writeEncrypted = false;
        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        /// <summary>
        /// Invoked when this particular SaveWriter writes AmanitaSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<AmanitaSaveData, string, string> AmanitaSaveWritten = delegate { };
        protected const string fileNameFormat = "{0}_0{1}.{2}";
        protected const string filePathFormat = "{0}/{1}";

        protected virtual void OnEnable()
        {
            
        }

        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful, or 
        /// false otherwise.
        /// </summary>
        public virtual bool WriteOneToDisk(SaveWriteArgs args)
        {
            // Safety.
            string saveFolder = SaveSystem.SaveDirectoryPaths[args.SaveDirectory];
            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

            SaveData saveData = args.SaveData;
            var dataToWrite = JsonUtility.ToJson(saveData, true);
            string fileName = string.Format(fileNameFormat, savePrefix, args.SlotNumber, fileExtension);
            var filePath = string.Format(filePathFormat, saveFolder, fileName);

            // For now, we won't worry about encryption
            if (!writeEncrypted)
                File.WriteAllText(filePath, dataToWrite, actualEncoding);

            else
            {
                WriteAsEncrypted();
                void WriteAsEncrypted()
                {
                    // Note: The binary-writing is not yet secure.
                    using Stream fileStream = File.Open(filePath, FileMode.Create);
                    using BinaryWriter writer = new BinaryWriter(fileStream, actualEncoding);
                    writer.Write(dataToWrite);
                }
            }

            return true;

        }


        /// <summary>
        /// Writes all the save datas to the passed save directory, returning true if successful,
        /// false otherwise.
        /// </summary>
        public virtual bool WriteAllToDisk(IList<SaveWriteArgs> args)
        {
            bool didWeSucceed = default;
            for (int i = 0; i < args.Count; i++)
            {
                SaveWriteArgs currentArgs = args[i];
                didWeSucceed = WriteOneToDisk(currentArgs);
                if (!didWeSucceed)
                {
                    break;
                }
            }

            return didWeSucceed;
        }

    }
}