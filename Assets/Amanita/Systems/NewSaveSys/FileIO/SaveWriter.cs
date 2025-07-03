using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;
using System.Threading.Tasks;
using Amanita.Collections;
using Amanita.IO;
using System.Threading;

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

        [SerializeField] protected bool deleteBackupsPostOverwrite = true;

        public virtual bool DeleteBackupsPostOverwrite
        {
            get => deleteBackupsPostOverwrite;
            set => deleteBackupsPostOverwrite = value;
        }

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        /// <summary>
        /// Invoked when this particular SaveWriter writes CompositeSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<SaveWriteResults> AmanitaSaveWritten = delegate { };

        protected override void OnEnable()
        {
            base.OnEnable();
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
        public virtual async Task<bool> WriteAllToDisk(IList<SaveWriteRequest> args, CancellationToken token = default)
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
        public virtual async Task<bool> WriteOneToDisk(SaveWriteRequest request, CancellationToken token = default)
        {
            // Safety.
            Validate(request);

            string saveFolder = GetFolderToAccess(request.BaseSaveDirectory),
                numFormatted = request.SlotNumber.ToString(SaveNumberFormat);

            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

            string fileName = string.Format(fileNameFormat, savePrefix,
                    numFormatted, fileExtension);
            string filePath = saveFolder + fileName;
            debugSaveFolder = saveFolder;

            debugFilePath = filePath;
            string backupFilePath = $"{filePath}{backupFileExtension}";

            await DoTheWriting().ConfigureAwait(false);
            async Task DoTheWriting()
            {
                DeleteOldBackup();
                void DeleteOldBackup()
                {
                    // So we can create a new, updated one when appropriate
                    if (File.Exists(backupFilePath))
                    {
                        IOUtils.UnityFileDelete(backupFilePath);
                    }
                }

                PrepForOverwriting();
                void PrepForOverwriting()
                {
                    bool areWeOverwriting = File.Exists(filePath);

                    if (areWeOverwriting)
                    {
                        PrepBackup();
                        void PrepBackup()
                        {
                            try
                            {
                                // For the sake of performance, we're renaming the file
                                IOUtils.UnityFileMove(filePath, backupFilePath);
                            }
                            catch (IOException ex)
                            {
                                // This can happen if the file is locked by another process,
                                // or if the file is read-only, or if the file is on a different
                                // filesystem that doesn't support renaming.
                                // In that case, we want to copy the file instead.
                                Debug.LogError($"Could not move file {filePath} to backup {backupFilePath}." +
                                    $"\nException: {ex.Message}");
                                File.Copy(filePath, backupFilePath);
                                throw ex;
                            }
                        }
                    }
                }

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

                        string everythingToWrite = $"{metaTextToWrite}{ReadWriteDelimiter}" +
                            $"{mainStateTextToWrite}{CompletionMarker}";
                        await File.WriteAllTextAsync(filePath, everythingToWrite, actualEncoding).ConfigureAwait(false);
                    }
                }

                else
                {
                    await WriteAsEncrypted();
                    async Task WriteAsEncrypted()
                    {
                        SaveDataSet saveDataSet = new SaveDataSet(request.SaveMetaData, request.MainState);
                        encryptionRequest.SaveDataSet = saveDataSet;
                        encryptionRequest.CompletionMarker = CompletionMarker;
                        IEncryptor correctEncryptor = encryptor as IEncryptor;
                        byte[] encryptedData = (byte[])correctEncryptor.GetOutput(encryptionRequest);
                        
                        await File.WriteAllBytesAsync(filePath, encryptedData);
                    }
                }

                OnWritingComplete();
                void OnWritingComplete()
                {
                    if (DeleteBackupsPostOverwrite && File.Exists(backupFilePath))
                    {
                        File.Delete(backupFilePath);
                    }
                }
            }

            AnnounceResults();
            void AnnounceResults()
            {
                writeResults.FilePath = filePath;
                writeResults.FileName = fileName;
                writeResults.SaveData = request.MainState as CompositeSaveData;
                writeResults.Success = true;
                writeResults.ErrorMessage = string.Empty;
                writeResults.Request = request;

                AmanitaSaveWritten(writeResults);
                SaveSysSignals.AmanitaSaveWritten.Invoke(writeResults);
            }

            return true;
        }

        protected BaseEncryptionRequest encryptionRequest = new BaseEncryptionRequest();
        protected SaveWriteResults writeResults = new SaveWriteResults(); // Caching this for performance

        protected string backupFileExtension = ".bak";
        public virtual string BackupFileExtension
        {
            get => backupFileExtension;
        }
        protected string tempFileExtension = ".tmp";

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

            bool validBaseDirectory = SaveSystem.S.SaveDirectoryPaths.ContainsKey(writeArgs.BaseSaveDirectory);
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

    public class BaseEncryptionRequest
    {
        public virtual SaveDataSet SaveDataSet { get; set; }
        public virtual string CompletionMarker { get; set; }
    }
}