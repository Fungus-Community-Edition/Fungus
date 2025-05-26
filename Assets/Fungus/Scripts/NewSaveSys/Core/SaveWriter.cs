using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        [SerializeField] protected bool writeEncrypted = false;
        public virtual bool WriteEncrypted
        {
            get => writeEncrypted;
            set => writeEncrypted = value;
        }

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        /// <summary>
        /// Invoked when this particular SaveWriter writes AmanitaSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<AmanitaSaveData, string, string> AmanitaSaveWritten = delegate { };
        protected const string fileNameFormat = "{0}_0{1}.{2}";
        protected const string filePathFormat = "{0}/{1}";

        public virtual string FileNameFormat => fileNameFormat;
        public virtual string FilePathFormat => filePathFormat;

        protected virtual void OnEnable()
        {
            
        }

        /// <summary>
        /// Writes all the save datas to the passed save directory, returning true if successful,
        /// false otherwise.
        /// </summary>
        public virtual bool WriteAllToDisk(IList<SaveWriteRequest> args)
        {
            bool didWeSucceed = default;
            for (int i = 0; i < args.Count; i++)
            {
                SaveWriteRequest currentArgs = args[i];
                didWeSucceed = WriteOneToDisk(currentArgs);
                if (!didWeSucceed)
                {
                    break;
                }
            }

            return didWeSucceed;
        }

        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful, or 
        /// false otherwise.
        /// </summary>
        public virtual bool WriteOneToDisk(SaveWriteRequest args)
        {
            // Safety.
            Validate(args);

            string saveFolder = string.Empty, stringDataToWrite = string.Empty,
                fileName = string.Empty, filePath = string.Empty;

            DecideDirectoriesAndSuch();
            void DecideDirectoriesAndSuch()
            {
                saveFolder = SaveSystem.SaveDirectoryPaths[args.BaseSaveDirectory];

                if (relativeSavePath.Count() > 0)
                {
                    saveFolder = Path.Combine(saveFolder, relativeSavePath);
                }
                Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

                SaveData saveData = args.SaveData;
                stringDataToWrite = JsonUtility.ToJson(saveData, true);
                // ^Might want to write a float array in the future, but for now, we just write the JSON string.
                fileName = string.Format(fileNameFormat, savePrefix, args.SlotNumber, fileExtension);
                filePath = string.Format(filePathFormat, saveFolder, fileName);
            }

            // For now, we won't worry about encryption
            if (!writeEncrypted)
                File.WriteAllText(filePath, stringDataToWrite, actualEncoding);

            else
            {
                WriteAsEncrypted();
                void WriteAsEncrypted()
                {
                    byte key = 0xAA;
                    byte[] encryptedData = actualEncoding.GetBytes(stringDataToWrite)
                        .Select(b => (byte)(b ^ key))
                        .ToArray(); // Simple XOR encryption to prevent casual snooping.
                    File.WriteAllBytes(filePath, encryptedData);
                }
            }

            AnnounceResults();
            void AnnounceResults()
            {
                SaveWriteResults results = new SaveWriteResults
                {
                    FilePath = filePath,
                    FileName = fileName,
                    SaveData = args.SaveData as AmanitaSaveData,
                    Success = true,
                    ErrorMessage = string.Empty,
                    Request = args
                };
                SaveSysSignals.AmanitaSaveWritten.Invoke(results);
            }
            return true;

        }

        /// <summary>
        /// If there's anything wrong, an exception will be thrown. Otherwise, returns true.
        /// </summary>
        /// <param name="writeArgs"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual bool Validate(SaveWriteRequest writeArgs)
        {
            string errorMessage = string.Empty;
            System.Exception exception = null;
            
            bool isNull = writeArgs.SaveData == null;
            if (isNull)
            {
                errorMessage += "SaveData is null. Cannot write to disk.\n";
                exception = new System.ArgumentNullException(nameof(writeArgs.SaveData), errorMessage);
                throw exception;
            }

            bool validSaveName = !string.IsNullOrEmpty(writeArgs.SaveName);
            if (string.IsNullOrEmpty(writeArgs.SaveName))
            {
                errorMessage += "SaveName is null or empty. Cannot write to disk.\n";
                exception = new System.ArgumentNullException(nameof(writeArgs.SaveName), errorMessage);
                throw exception;
            }

            bool validBaseDirectory = SaveSystem.SaveDirectoryPaths.ContainsKey(writeArgs.BaseSaveDirectory);
            if (!validBaseDirectory)
            {
                errorMessage += $"BaseSaveDirectory {writeArgs.BaseSaveDirectory} is not a valid SaveDirectoryType.\n";
                exception = new System.ArgumentException(errorMessage, nameof(writeArgs.BaseSaveDirectory));
                throw exception;
            }

            bool validSaveNumber = writeArgs.SlotNumber >= 0;
            if (!validSaveNumber)
            {
                errorMessage += "SlotNumber is negative. Cannot write to disk.\n";
                exception = new System.ArgumentOutOfRangeException(nameof(writeArgs.SlotNumber), errorMessage);
                throw exception;
            }

            bool validRelativeDirectory = !string.IsNullOrEmpty(relativeSavePath);
            if (!validRelativeDirectory)
            {
                errorMessage += "RelativeSavePath is null or empty. Cannot write to disk.\n";
                exception = new System.ArgumentNullException(nameof(relativeSavePath), errorMessage);
                throw exception;
            }


            bool didWeSucceed = !isNull && validSaveName &&
                validBaseDirectory && validSaveNumber &&
                validRelativeDirectory;

            if (!didWeSucceed)
            {
                throw exception;
            }

            return didWeSucceed;
        }

        

    }
}