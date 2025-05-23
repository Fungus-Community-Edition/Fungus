using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for writing save data to disk.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSaveWriter", menuName = "Amanita/SaveSys/SaveWriter")]
    public class SaveWriter : SaveDiskAccessor
    {
        #region Fields

        [Tooltip("For when you want to make it harder for players to cheat by editing the save data. Experimental.")]
        [SerializeField] protected bool writeEncrypted;

        protected System.Text.Encoding actualEncoding;

        /// <summary>
        /// Invoked when this particular SaveWriter writes AmanitaSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<AmanitaSaveData, string, string> AmanitaSaveWritten = delegate { };
        protected const string fileNameFormat = "{0}_0{1}.{2}";
        protected const string filePathFormat = "{0}/{1}";

        #endregion

        #region Methods

        protected virtual void OnEnable()
        {
            
        }

        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful, or 
        /// false otherwise.
        /// </summary>
        public virtual bool WriteOneToDisk(AmanitaSaveData saveData, string saveDir)
        {
            // Safety.
            if (!Directory.Exists(saveDir))
            {
                var messageFormat =
                @"Could not write save to {0}; that directory does not exist.";
                var message = string.Format(messageFormat, saveDir);
                Debug.LogError(message);
                return false;
            }

            var dataToWrite = JsonUtility.ToJson(saveData, true);

            // Write the file at the appropriate directory with the appropriate writing method.
            var fileName = $"{savePrefix}.{fileExtension}";
            var filePath = $"{saveDir}/{fileName}";

            if (!writeEncrypted)
                File.WriteAllText(filePath, dataToWrite, actualEncoding);

            else
            {
                WriteAsEncrypted();
                void WriteAsEncrypted()
                {
                    // Note: The binary-writing is not yet secure.
                    using (Stream fileStream = File.Open(filePath, FileMode.Create))
                    {
                        using (BinaryWriter writer = new BinaryWriter(fileStream, actualEncoding))
                        {
                            writer.Write(dataToWrite);
                        }
                    }
                }
            }

            SaveSysSignals.AmanitaSaveWritten.Invoke(saveData, filePath, fileName);
            AmanitaSaveWritten.Invoke(saveData, filePath, fileName);

            return true;

        }

        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful,
        /// and false otherwise. If successful, the saved file's location is put into the passed outputDir.
        /// </summary>
        public virtual bool WriteOneToDisk(AmanitaSaveData saveData, string saveDir,
            out string outputDir)
        {
            var success = WriteOneToDisk(saveData, saveDir);
            outputDir = "";

            if (success)
            {
                string fileName = $"{savePrefix}.{fileExtension}";
                string filePath = $"{saveDir}/{fileName}";
                outputDir = filePath;
            }

            return success;
        }

        /// <summary>
        /// Writes all the save datas to the passed save directory, returning true if successful,
        /// false otherwise.
        /// </summary>
        public virtual bool WriteAllToDisk(IList<AmanitaSaveData> saveDatas, string saveDir)
        {
            var success = false;

            for (int i = 0; i < saveDatas.Count; i++)
            {
                success = WriteOneToDisk(saveDatas[i], saveDir);

                if (!success)
                    return false;
            }

            return true;
        }

        #endregion
    }
}