using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{ 
    public class SaveSystem : MonoBehaviour
    {
        [SerializeField] protected SaveEncoder[] mainEncoders = new SaveEncoder[0];
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

            saveManager.SaveDirType = saveDirectoryType;
            saveManager.RegisterMultiMainEncoders(mainEncoders);
            saveManager.SaveWriter = saveWriter;
            saveManager.SaveReader = saveReader;
        }

        protected SaveManager saveManager = new SaveManager();

        public virtual void RegisterSave(CompositeSaveData save)
        {

        }

        public virtual void LoadSave(string saveName)
        {
            saveManager.LoadSave(saveName);
        }

        public virtual void DeleteSave(string saveName)
        {
            saveManager.DeleteSave(saveName);
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