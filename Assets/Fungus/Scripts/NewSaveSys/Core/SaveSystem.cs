using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

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
            if (_s != null && _s != this)
            {
                Debug.LogWarning("SaveSystem already exists. Destroying the new one.");
                Destroy(this.gameObject);
                return;
            }

            _s = this;

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
                IList<ScriptableObject> invalidEncoders =   (from elem in mainEncoders
                                                            where !(elem is IMainSaveCodec)
                                                            where elem != null
                                                            select elem).ToList();
                for (int i = 0; i < invalidEncoders.Count; i++)
                {
                    ScriptableObject currentInvalid = invalidEncoders[i];

                    string encoderName = currentInvalid.name;
                    string errorMessage = $"Main encoder {encoderName} is not a valid one. Make sure that everything in the mainEncoders list implements IMainSaveCodec.";
                    Debug.LogError(errorMessage);
                }
            }

            IList<IMainSaveCodec> validatedEncoders = (from elem in mainEncoders
                                                       where elem is IMainSaveCodec
                                                       select elem as IMainSaveCodec).ToList();

            PrepSaveManager();
            void PrepSaveManager()
            {
                FileSaveRepository repo = new FileSaveRepository();
                repo.Init(saveReader, saveWriter);
                saveRepo = repo;
                saveManager = new SaveManager(saveRepo)
                {
                    SaveRelativePath = "/Saves",
                    SaveDirType = saveDirectoryType,
                };
                saveManager.RegisterMultiMainCodecs(validatedEncoders);
            }
        }

        public static SaveSystem S
        {
            get
            {
                if (_s == null)
                {
                    GameObject holder = new GameObject("SaveSystem");
                    _s = holder.AddComponent<SaveSystem>();
                }

                return _s;
            }
        }
        protected static SaveSystem _s;

        protected ISaveRepository saveRepo;

        protected SaveManager saveManager;

        public virtual Task SaveTo(int slotNum)
        {
            return saveManager.SaveTo(slotNum);
        }

        public virtual Task<CompositeSaveData> LoadSave(int slotNum)
        {
            return saveRepo.LoadMainSaveAsync(slotNum);
        }

        public virtual void DeleteSave(int slotNum)
        {
            saveManager.DeleteSave(slotNum);    
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