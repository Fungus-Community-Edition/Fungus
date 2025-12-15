using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using FileEncoding = System.Text.Encoding;
using System.Threading.Tasks;
using Amanita.IO;
using System.Threading;
using FullSerializer;
using Amanita.FSExt;
using Action = System.Action;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for writing save data to disk.
    /// </summary>
    [SaveSysDisplayName("Save Writer (Amanita Default)")]
    [CreateAssetMenu(fileName = "NewSaveWriter", menuName = "Amanita/SaveSys/SaveWriter")]
    public class SaveWriter : SaveDiskAccessor, ISaveWriter
    {
        [SerializeField] private ScriptableObject encryptor;

        [SerializeField] private bool deleteBackupsPostOverwrite = true;

        public virtual ScriptableObject Encryptor
        {
            get => encryptor;
            set => encryptor = value;
        }

        public virtual bool DeleteBackupsPostOverwrite
        {
            get => deleteBackupsPostOverwrite;
            set => deleteBackupsPostOverwrite = value;
        }

        private readonly FileEncoding actualEncoding = FileEncoding.UTF8;

        /// <summary>
        /// Invoked when this particular SaveWriter writes CompositeSaveData.
        /// Params: saveData, filePath, fileName
        /// </summary>
        public UnityAction<SaveWriteResults> AmanitaSaveWritten = delegate { };

        protected override void OnEnable()
        {
            base.OnEnable();

            if (encryptor == null)
            {
                encryptor = DefaultAmanitaAssets.Encryptor;
            }
        }

        public virtual bool WriteAllToDisk(IList<SaveWriteRequest> args, Action onComplete = null)
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
            onComplete?.Invoke();
            return didWeSucceed;
        }

        public virtual bool WriteOneToDisk(SaveWriteRequest request, Action onComplete = null)
        {
            Task<bool> writeTask = WriteOneToDiskAsync(request);
            writeTask.Wait();
            bool result = writeTask.Result;
            onComplete?.Invoke();
            return result;
        }

        /// <summary>
        /// Writes all the save datas to the passed save directory, returning true if successful,
        /// false otherwise.
        /// </summary>
        public virtual async Task<bool> WriteAllToDiskAsync(IList<SaveWriteRequest> args, CancellationToken token = default)
        {
            bool didWeSucceed = default;
            for (int i = 0; i < args.Count; i++)
            {
                SaveWriteRequest currentArgs = args[i];
                didWeSucceed = await WriteOneToDiskAsync(currentArgs);
                if (!didWeSucceed)
                {
                    break;
                }
            }

            return didWeSucceed;
        }

        private string debugSaveFolder, debugFilePath;
        /// <summary>
        /// Writes the passed save data to the passed save directory, returning true if successful, or 
        /// false otherwise.
        /// </summary>
        public virtual async Task<bool> WriteOneToDiskAsync(SaveWriteRequest request, CancellationToken token = default)
        {
            // Safety.
            Validate(request);

            string saveFolder = GetSaveFolderPath(request.BaseSaveDirectory);
            Directory.CreateDirectory(saveFolder); // In case it doesn't exist.

            string filePath = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
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
                                string errorMessage = $"Could not move file {filePath} to backup {backupFilePath}." +
                                    $"\nException: {ex.Message}";
                                Debug.LogError(errorMessage);
                                File.Copy(filePath, backupFilePath);
                                throw new IOException(errorMessage);
                            }
                        }
                    }
                }

                if (!ExpectEncryption)
                {
                    await WriteFullJsonTextToFile();
                    async Task WriteFullJsonTextToFile()
                    {
                        string metaTextToWrite, mainStateTextToWrite;

                        DecideTextToWrite();
                        void DecideTextToWrite()
                        {
                            ISaveMetaData meta = request.SaveMetaData;
                            metaTextToWrite = Serializer.ToJson(meta, true);

                            ISaveData saveData = request.MainState;
                            mainStateTextToWrite = Serializer.ToJson(saveData, true);
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

                        string backupMetaPath = backupFilePath + ".meta";
                        if (File.Exists(backupMetaPath))
                        {
                            File.Delete(backupMetaPath);
                        }
                    }
                }
            }

            AnnounceResults();
            void AnnounceResults()
            {
                writeResults.FilePath = filePath;
                writeResults.FileName = GetSaveFileName(request.SlotNumber);
                writeResults.SaveData = request.MainState as CompositeSaveData;
                writeResults.Success = true;
                writeResults.ErrorMessage = string.Empty;
                writeResults.Request = request;

                AmanitaSaveWritten(writeResults);
                SaveSysSignals.AmanitaSaveWritten.Invoke(writeResults);
            }

            return true;
        }

        private static fsSerializer Serializer => AmanitaManager.DefaultSerializer;
        private readonly BaseEncryptionRequest encryptionRequest = new BaseEncryptionRequest();
        private readonly SaveWriteResults writeResults = new SaveWriteResults(); // Caching this for performance

        private static readonly string backupFileExtension = ".bak";
        public virtual string BackupFileExtension
        {
            get => backupFileExtension;
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

            string baseDirectory = SaveSystem.S.GetSaveDirectory(writeArgs.BaseSaveDirectory);
            bool validBaseDirectory = !string.IsNullOrEmpty(baseDirectory);
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
                encryptor = DefaultAmanitaAssets.Encryptor;
                Debug.LogError($"Tried to assign a Scriptable Object that does not implement IEncryptor. Reverting to default.");
            }
        }

    }

    public class BaseEncryptionRequest
    {
        public virtual SaveDataSet SaveDataSet { get; set; }
        public virtual string CompletionMarker { get; set; }
    }

    public interface ISaveWriter
    {
        bool WriteOneToDisk(SaveWriteRequest request, Action onComplete = null);
        bool WriteAllToDisk(IList<SaveWriteRequest> args, Action onComplete = null);
        Task<bool> WriteOneToDiskAsync(SaveWriteRequest request, CancellationToken token = default);
        Task<bool> WriteAllToDiskAsync(IList<SaveWriteRequest> args, CancellationToken token = default);
    }
}