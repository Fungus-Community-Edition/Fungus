using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
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

        [SerializeField] protected ScriptableObject encryptor;

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        /// <summary>
        /// Invoked when this particular SaveWriter writes CompositeSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<SaveWriteResults> AmanitaSaveWritten = delegate { };

        protected virtual void OnEnable()
        {
            EnsureWeHaveBackupEncryptor();
            void EnsureWeHaveBackupEncryptor()
            {
                if (defaultEncryptor == null)
                {
                    defaultEncryptor = CreateInstance<Encryptor>();
                }
            }

            if (encryptor == null)
            {
                encryptor = CreateInstance<Encryptor>();
            }
        }

        protected Encryptor defaultEncryptor;

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
        public virtual bool WriteOneToDisk(SaveWriteRequest request)
        {
            // Safety.
            Validate(request);

            string saveFolder = string.Empty, fileName = string.Empty,
                filePath = string.Empty;

            RegisterAndEnsureFullPath();
            void RegisterAndEnsureFullPath()
            {
                saveFolder = SaveSystem.SaveDirectoryPaths[request.BaseSaveDirectory];

                // Need to make sure we have that slash at the end
                if (!saveFolder.EndsWith("/") && !saveFolder.EndsWith("\\"))
                {
                    saveFolder += "\\";
                }

                bool thereIsRelativePathToConsider = relativeSavePath.Count() > 1;
                if (thereIsRelativePathToConsider)
                {
                    saveFolder = Path.Combine(saveFolder, relativeSavePath);
                }
                Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

                fileName = string.Format(fileNameFormat, savePrefix,
                    request.SlotNumber.ToString("D3"), fileExtension);
                filePath = string.Format(filePathFormat, saveFolder, fileName);
            }

            if (!writeEncrypted)
            {
                WriteFullJsonTextToFile();
                void WriteFullJsonTextToFile()
                {
                    string metaTextToWrite;
                    string mainStateTextToWrite;

                    DecideTextToWrite();
                    
                    void DecideTextToWrite()
                    {
                        ISaveMetaData meta = request.SaveMetaData;
                        metaTextToWrite = JsonUtility.ToJson(meta, true);

                        ISaveData saveData = request.MainState;
                        mainStateTextToWrite = JsonUtility.ToJson(saveData, true);
                    }

                    string everythingToWrite = $"{metaTextToWrite}{ReadWriteDelimiter}{mainStateTextToWrite}";
                    File.WriteAllText(filePath, everythingToWrite, actualEncoding);
                }
            }

            else
            {
                WriteAsEncrypted();
                
                void WriteAsEncrypted()
                {
                    SaveDataSet saveDataSet = new SaveDataSet(request.SaveMetaData, request.MainState);
                    IEncryptor correctEncryptor = encryptor as IEncryptor;
                    byte[] encryptedData = (byte[])correctEncryptor.GetOutput(saveDataSet);
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
                    SaveData = request.MainState as CompositeSaveData,
                    Success = true,
                    ErrorMessage = string.Empty,
                    Request = request
                };

                AmanitaSaveWritten(results);
                SaveSysSignals.AmanitaSaveWritten.Invoke(results);
            }
            return true;

        }

        /// <summary>
        /// If there's anything wrong, an exception will be thrown. Otherwise, returns true.
        /// </summary>
        protected virtual bool Validate(SaveWriteRequest writeArgs)
        {
            string errorMessage = string.Empty;
            System.Exception exception = null;
            
            bool isNull = writeArgs.MainState == null;
            if (isNull)
            {
                errorMessage += "SaveData is null. Cannot write to disk.\n";
                exception = new System.ArgumentNullException(nameof(writeArgs.MainState), errorMessage);
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

            bool didWeSucceed = !isNull && validSaveName &&
                validBaseDirectory && validSaveNumber;

            if (!didWeSucceed)
            {
                throw exception;
            }

            return didWeSucceed;
        }

        protected virtual void OnValidate()
        {
            bool wrongTypeOfSOAssigned = encryptor != null && encryptor is not IEncryptor;
            if (wrongTypeOfSOAssigned)
            {
                encryptor = defaultEncryptor;
                Debug.LogError($"Tried to assign a Scriptable Object that does not implement IEncryptor. Reverting to default.");
            }

            if (string.IsNullOrEmpty(relativeSavePath))
            {
                relativeSavePath = "/";
            }
        }

    }
}