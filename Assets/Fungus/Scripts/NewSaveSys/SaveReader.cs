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

        protected virtual void OnEnable()
        {
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
            string filePath = FileUtils.GetPathToFile(request.BaseSaveDirectory, request.SlotNumber, this); //GetFullFilePath(request);
            Validate(filePath);

            bool writtenAsPlainText = !readEncrypted;
            byte[] rawBytes = await File.ReadAllBytesAsync(filePath, cancelToken);
            object[] infoForDecryptor = new object[] { rawBytes, writtenAsPlainText };

            SaveMetaData result = (SaveMetaData)usableDecryptor.DecryptMeta(infoForDecryptor);
            return result;
        }

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
            string filePath = GetFullFilePath(request);
            Validate(filePath);

            bool writtenAsPlainText = !readEncrypted;
            byte[] rawBytes = await File.ReadAllBytesAsync(filePath, cancelToken);
            object[] infoForDecryptor = new object[] { rawBytes, writtenAsPlainText };

            CompositeSaveData result = (CompositeSaveData) usableDecryptor.DecryptMainState(infoForDecryptor);
            
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

    
}