using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
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

        [SerializeField] protected ScriptableObject decryptor;

        protected FileEncoding actualEncoding = FileEncoding.UTF8;

        protected override void OnEnable()
        {
            base.OnEnable();
            PrepDefaultDecryptor();
            void PrepDefaultDecryptor()
            {
                if (defaultDecryptor == null)
                {
                    defaultDecryptor = CreateInstance<Decryptor>();
                }
            }

            if (decryptor == null)
            {
                decryptor = defaultDecryptor;
            }

            usableDecryptor = decryptor as IDecryptor;
        }

        protected Decryptor defaultDecryptor;
        protected IDecryptor usableDecryptor;

        public virtual async Task<ISaveMetaData> ReadMetadataFromDisk(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            await PrepDecryptionRequest(request, cancelToken);
            SaveMetaData result = (SaveMetaData)usableDecryptor.DecryptMeta(decryptionRequest);
            return result;
        }

        protected virtual async Task PrepDecryptionRequest(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            string filePath = FileUtils.GetPathToFile(request.BaseSaveDirectory, request.SlotNumber, this);
            Validate(filePath);
            bool writtenAsPlainText = !readEncrypted;
            byte[] rawBytes = await File.ReadAllBytesAsync(filePath, cancelToken);
            decryptionRequest.RawBytes = rawBytes;
            decryptionRequest.WrittenAsPlainText = writtenAsPlainText;
            decryptionRequest.CompletionMarker = SaveDiskAccessor.CompletionMarker;
        }

        protected BaseDecryptionRequest decryptionRequest = new BaseDecryptionRequest();
        protected virtual string GetFullFilePath(SaveReadRequest request)
        {
            string saveFolderPath = FileUtils.GetPathToFolder(request.BaseSaveDirectory, RelativeSavePath);
            
            string numFormatted = request.SlotNumber.ToString(saveNumberFormat);
            string fileName = string.Format(fileNameFormat, savePrefix,
                numFormatted, fileExtension);
            string filePath = saveFolderPath + fileName;
            return filePath;
        }

        protected virtual void Validate(string filePath)
        {
            if (!File.Exists(filePath))
            {
                string fileName = Path.GetFileName(filePath);
                string errorMessage = $"Cannot read metadata of file {fileName}, because it is just like Santa Claus: it doesn't exist";
                throw new FileNotFoundException(errorMessage);
            }
        }

        public virtual async Task<CompositeSaveData> ReadMainSaveDataFromDisk(SaveReadRequest request,
            CancellationToken cancelToken = default)
        {
            await PrepDecryptionRequest(request, cancelToken);
            string filePath = GetFullFilePath(request);
            CompositeSaveData result = (CompositeSaveData) usableDecryptor.DecryptMainState(decryptionRequest);
            
            return result;
        }

        public virtual string GetSavePath(SaveReadRequest request)
        {
            string result = GetFullFilePath(request);
            return result;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            bool wrongTypeOfSOAssigned = decryptor != null && decryptor is not IDecryptor;
            if (wrongTypeOfSOAssigned)
            {
                decryptor = defaultDecryptor;
                Debug.LogError($"Tried to assign a Scriptable Object that does not implement IDecryptor. Reverting to default.");
            }
        }
    }

    public class BaseDecryptionRequest
    {
        public byte[] RawBytes { get; set; }
        public bool WrittenAsPlainText { get; set; }
        public string CompletionMarker { get; set; }
    }

}