using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
{ 
    public class SaveSystem : MonoBehaviour
    {

        [SerializeField] protected ScriptableObject[] mainEncoders = new ScriptableObject[] { };
        [SerializeField] protected SaveWriter saveWriter = null;
        [SerializeField] protected SaveReader saveReader = null;

        [Tooltip("In WebGL, things will be saved to PlayerPrefs due to the file system limitations web browsers have. In which case, this field won't make a difference.")]
        [SerializeField] protected SaveDirectoryType saveDirectoryType = SaveDirectoryType.DataPath;
        
        public virtual SaveDirectoryType SaveDirectoryType { get { return saveDirectoryType; } }

        protected virtual void Awake()
        {
            CheckForSaveWriterAndReader();
            void CheckForSaveWriterAndReader()
            {
                if (saveWriter == null)
                {
                    Debug.LogWarning("SaveWriter is not set. Going with the default.");
                    saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
                }

                if (saveReader == null)
                {
                    Debug.LogWarning("SaveReader is not set. Going with the default.");
                    saveReader = ScriptableObject.CreateInstance<SaveReader>();
                }
            }

            ValidateEncoders();
            void ValidateEncoders()
            {
                for (int i = 0; i < mainEncoders.Length; i++)
                {
                    ScriptableObject currentMain = mainEncoders[i];
                    bool isValid = currentMain is IMainSaveCodec;
                    string encoderName = string.Empty;
                    if (currentMain != null)
                    {
                        encoderName = currentMain.name;
                    }
                    if (!isValid)
                    {
                        string errorMessage = $"Main encoder {encoderName} is not a valid one. Make sure that everything in the mainEncoders list implements IMainSaveEncoder.";
                        throw new System.InvalidOperationException(errorMessage);
                    }
                }
            }

            saveManager.SaveDirType = saveDirectoryType;

            IList<IMainSaveCodec> validatedEncoders = new List<IMainSaveCodec>();
            for (int i = 0; i < mainEncoders.Length; i++)
            {
                ScriptableObject currentMain = mainEncoders[i];
                validatedEncoders.Add(currentMain as IMainSaveCodec);
            }

            saveManager.RegisterMultiMainEncoders(validatedEncoders);
            saveManager.SaveWriter = saveWriter;
            saveManager.SaveReader = saveReader;
        }

        protected SaveManager saveManager = new SaveManager();

        public virtual Task SaveTo(int slotNum)
        {
            return saveManager.SaveTo(slotNum);
        }

        public virtual Task<CompositeSaveData> LoadSave(int slotNum)
        {
            return saveManager.LoadSave(slotNum);
        }

        public virtual Task DeleteSave(int slotNum)
        {
            return saveManager.DeleteSave(slotNum);    
        }

        public static IDictionary<SaveDirectoryType, string> SaveDirectoryPaths;

        public static void InitPaths()
        {
            SaveDirectoryPaths =
            new Dictionary<SaveDirectoryType, string>
            {
                { SaveDirectoryType.DataPath, Application.dataPath },
                { SaveDirectoryType.PersistentDataPath, Application.persistentDataPath },
                { SaveDirectoryType.StreamingAssetsPath, Application.streamingAssetsPath }
            };
        }
    }

    public enum SaveDirectoryType
    {
        Null,
        DataPath,
        PersistentDataPath,
        StreamingAssetsPath,
    }
}