using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;
using System.Threading.Tasks;

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
        public virtual async Task<bool> WriteAllToDisk(IList<SaveWriteRequest> args)
        {
            bool didWeSucceed = default;
            for (int i = 0; i < args.Count; i++)
            {
                SaveWriteRequest currentArgs = args[i];
                didWeSucceed = await WriteOneToDisk(currentArgs);
                if (!didWeSucceed)
                {
                    break;
                }
            }

            return didWeSucceed;
        }

        protected string debugSaveFolder, debugFilePath;
        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful, or 
        /// false otherwise.
        /// </summary>
        public virtual async Task<bool> WriteOneToDisk(SaveWriteRequest request)
        {
            // Safety.
            Validate(request);

            string saveFolder = GetFolderToAccess(request.BaseSaveDirectory),
                numFormatted = request.SlotNumber.ToString(SaveNumberFormat),
                fileName = string.Empty,
                filePath = string.Empty;

            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

            fileName = string.Format(fileNameFormat, savePrefix,
                    numFormatted, fileExtension);
            filePath = saveFolder + fileName;
            debugSaveFolder = saveFolder;

            debugFilePath = filePath;

            await DoTheWriting().ConfigureAwait(false);
            async Task DoTheWriting()
            {
                if (!writeEncrypted)
                {
                    await WriteFullJsonTextToFile();
                    async Task WriteFullJsonTextToFile()
                    {
                        string metaTextToWrite, mainStateTextToWrite;

                        DecideTextToWrite();
                        void DecideTextToWrite()
                        {
                            ISaveMetaData meta = request.SaveMetaData;
                            metaTextToWrite = JsonUtility.ToJson(meta, true);

                            ISaveData saveData = request.MainState;
                            mainStateTextToWrite = JsonUtility.ToJson(saveData, true);
                        }

                        string everythingToWrite = $"{metaTextToWrite}{ReadWriteDelimiter}{mainStateTextToWrite}";
                        await File.WriteAllTextAsync(filePath, everythingToWrite, actualEncoding).ConfigureAwait(false);
                    }
                }

                else
                {
                    await WriteAsEncrypted();
                    async Task WriteAsEncrypted()
                    {
                        SaveDataSet saveDataSet = new SaveDataSet(request.SaveMetaData, request.MainState);
                        IEncryptor correctEncryptor = encryptor as IEncryptor;
                        byte[] encryptedData = (byte[])correctEncryptor.GetOutput(saveDataSet);
                        await File.WriteAllBytesAsync(filePath, encryptedData);
                    }
                }
            }

            Debug.Log("Right before AnnounceResults() in SaveWriter.WriteOneToDisk()");
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

            bool didWeSucceed = !isNull && validBaseDirectory && validSaveNumber;

            if (!didWeSucceed)
            {
                throw exception;
            }

            return didWeSucceed;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

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