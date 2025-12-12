using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using FileEncoding = System.Text.Encoding;

namespace Amanita.SaveSys
{
    [SaveSysDisplayName("Save Reader (Amanita Default)")]
    [CreateAssetMenu(fileName = "NewSaveReader", menuName = "Amanita/SaveSys/SaveReader")]
    public class SaveReader : SaveDiskAccessor, ISaveReader
    {
        [SerializeField] private ScriptableObject decryptor;
        public ScriptableObject Decryptor
        {
            get => decryptor;
            set
            {
                decryptor = value;
                usableDecryptor = decryptor as IDecryptor;
            }
        }

        private readonly FileEncoding actualEncoding = FileEncoding.UTF8;

        protected override void OnEnable()
        {
            base.OnEnable();
            HandleDecryptorField();
            void HandleDecryptorField()
            {
                EnsureDefaultDecryptor();
                void EnsureDefaultDecryptor()
                {
                    if (defaultDecryptor == null)
                    {
                        defaultDecryptor = DefaultAmanitaAssets.Decryptor;
                    }
                }

                if (decryptor == null)
                {
                    decryptor = DefaultAmanitaAssets.Decryptor;
                }
                if (decryptor == null)
                {
                    decryptor = defaultDecryptor;
                }

                usableDecryptor = decryptor as IDecryptor;
            }
        }

        private static Decryptor defaultDecryptor;
        private IDecryptor usableDecryptor;

        public virtual async Task<ISaveMetaData> ReadMetadataFromDiskAsync(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            await PrepDecryptionRequestAsync(request, cancelToken);
            SaveMetaData result = (SaveMetaData)usableDecryptor.DecryptMeta(decryptionRequest);
            return result;
        }

        private async Task PrepDecryptionRequestAsync(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            string filePath = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
            Validate(filePath);
            bool writtenAsPlainText = !ExpectEncryption;
            byte[] rawBytes = await ReadAllBytesAsync(filePath, cancelToken);
            decryptionRequest.RawBytes = rawBytes;
            decryptionRequest.WrittenAsPlainText = writtenAsPlainText;
            decryptionRequest.CompletionMarker = SaveDiskAccessor.CompletionMarker;
        }

        protected virtual async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancelToken)
        {
            return await File.ReadAllBytesAsync(filePath, cancelToken).ConfigureAwait(false);
        }

        private readonly BaseDecryptionRequest decryptionRequest = new BaseDecryptionRequest();

        private void Validate(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                string errorMessage = $"Cannot read metadata of file {fileName}, because it is just like Santa Claus: it doesn't exist";
                throw new FileNotFoundException(errorMessage);
            }
        }

        public virtual async Task<CompositeSaveData> ReadMainSaveDataFromDiskAsync(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            await PrepDecryptionRequestAsync(request, cancelToken);
            string filePath = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
            CompositeSaveData result = (CompositeSaveData) usableDecryptor.DecryptMainState(decryptionRequest);
            
            return result;
        }

        public virtual string GetSavePath(SaveReadRequest request)
        {
            string result = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (decryptor == null)
            {
                decryptor = DefaultAmanitaAssets.Decryptor;
            }
            bool wrongTypeOfSOAssigned = decryptor != null && decryptor is not IDecryptor;
            if (wrongTypeOfSOAssigned)
            {
                decryptor = defaultDecryptor;
                Debug.LogError($"Tried to assign a Scriptable Object that does not implement IDecryptor. Reverting to default.");
            }
        }
        
        public virtual IList<ISaveMetaData> ReadAllMetaDatasFromFolder(SaveDirectoryType dirType)
        {
            IList<ISaveMetaData> result = new List<ISaveMetaData>();
            string folderPath = GetSaveFolderPath(dirType);
            if (!Directory.Exists(folderPath))
            {
                string logMessage = $"Cannot read any meta datas from folder at path {folderPath}, because that " +
                    $"folder does not exist.";
                Debug.LogWarning(logMessage);
            }
            else
            {
                string[] saveFilesFound = Directory.GetFiles(folderPath, $"*.{storageSettings.FileExtension}");
                foreach (var file in saveFilesFound)
                {
                    byte[] rawBytes = File.ReadAllBytes(file);
                    decryptionRequest.RawBytes = rawBytes;
                    decryptionRequest.WrittenAsPlainText = !ExpectEncryption;
                    decryptionRequest.CompletionMarker = SaveDiskAccessor.CompletionMarker;
                    var meta = (SaveMetaData)usableDecryptor.DecryptMeta(decryptionRequest);
                    result.Add(meta);
                }
            }
            return result;
        }

        public virtual async Task<IList<ISaveMetaData>> ReadAllMetaDatasFromFolderAsync(SaveDirectoryType dirType,
            CancellationToken cancelToken = default)
        {
            IList<ISaveMetaData> result = new List<ISaveMetaData>();
            string folderPath = GetSaveFolderPath(dirType);
            if (!Directory.Exists(folderPath))
            {
                string logMessage = $"Cannot read any meta datas from folder at path {folderPath}, because that " +
                    $"folder does not exist.";
                Debug.LogWarning(logMessage);
            }
            else
            {
                string[] saveFilesFound = Directory.GetFiles(folderPath, $"*.{storageSettings.FileExtension}");
                foreach (var file in saveFilesFound)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    byte[] rawBytes = await ReadAllBytesAsync(file, cancelToken);
                    decryptionRequest.RawBytes = rawBytes;
                    decryptionRequest.WrittenAsPlainText = !ExpectEncryption;
                    decryptionRequest.CompletionMarker = SaveDiskAccessor.CompletionMarker;
                    var meta = (SaveMetaData)usableDecryptor.DecryptMeta(decryptionRequest);
                    result.Add(meta);
                }
            }
            return result;
        }

        public virtual ISaveMetaData ReadMetadataFromDisk(SaveReadRequest request, Action onComplete = null)
        {
            PrepDecryptionRequest(request);
            var result = (SaveMetaData)usableDecryptor.DecryptMeta(decryptionRequest);
            onComplete ??= delegate { };
            onComplete();
            return result;
        }

        private void PrepDecryptionRequest(SaveReadRequest request)
        {
            string filePath = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
            Validate(filePath);
            bool writtenAsPlainText = !ExpectEncryption;
            byte[] rawBytes = File.ReadAllBytes(filePath);
            decryptionRequest.RawBytes = rawBytes;
            decryptionRequest.WrittenAsPlainText = writtenAsPlainText;
            decryptionRequest.CompletionMarker = SaveDiskAccessor.CompletionMarker;
        }

        public virtual CompositeSaveData ReadMainSaveDataFromDisk(SaveReadRequest request, Action onComplete = null)
        {
            PrepDecryptionRequest(request);
            string filePath = GetSaveFilePath(request.BaseSaveDirectory, request.SlotNumber);
            CompositeSaveData result = (CompositeSaveData)usableDecryptor.DecryptMainState(decryptionRequest);
            return result;
        }

    }

    public class BaseDecryptionRequest
    {
        public byte[] RawBytes { get; set; }
        public bool WrittenAsPlainText { get; set; }
        public string CompletionMarker { get; set; }
    }

    public interface ISaveReader
    {
        ISaveMetaData ReadMetadataFromDisk(SaveReadRequest request, Action onComplete = null);
        Task<ISaveMetaData> ReadMetadataFromDiskAsync(SaveReadRequest request,
            CancellationToken cancelToken = default);

        CompositeSaveData ReadMainSaveDataFromDisk(SaveReadRequest request, Action onComplete = null);
        Task<CompositeSaveData> ReadMainSaveDataFromDiskAsync(SaveReadRequest request,
            CancellationToken cancelToken = default);

        IList<ISaveMetaData> ReadAllMetaDatasFromFolder(SaveDirectoryType dirType);

        Task<IList<ISaveMetaData>> ReadAllMetaDatasFromFolderAsync(SaveDirectoryType dirType,
            CancellationToken cancelToken = default);
    }

}