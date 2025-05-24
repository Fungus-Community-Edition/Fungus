using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{ 
    public class SaveSystem : MonoBehaviour
    {
        [SerializeField] protected SaveEncoder[] encoders = new SaveEncoder[0];
        [SerializeField] protected SaveWriter saveWriter = null;

        [Tooltip("In WebGL, things will be saved to PlayerPrefs due to the file system limitations web browsers have. In which case, this field won't make a difference.")]
        [SerializeField] protected SaveDirectoryType saveDirectoryType = SaveDirectoryType.DataPath;
        

        protected virtual void Awake()
        {
            if (saveWriter == null)
            {
                Debug.LogWarning("SaveWriter is not set. Going with the default.");
                saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
            }
        }

        protected SaveManager saveManager = new SaveManager();

        public virtual void RegisterSave(string saveName, SaveData saveData)
        {
            saveManager.RegisterSave(saveName, saveData);
        }

        public virtual void LoadSave(string saveName)
        {
            saveManager.LoadSave(saveName);
        }

        public virtual void DeleteSave(string saveName)
        {
            saveManager.DeleteSave(saveName);
        }

        public static IDictionary<SaveDirectoryType, string> SaveDirectoryPaths = 
            new Dictionary<SaveDirectoryType, string>
        {
            { SaveDirectoryType.DataPath, Application.dataPath },
            { SaveDirectoryType.PersistentDataPath, Application.persistentDataPath },
            { SaveDirectoryType.StreamingAssetsPath, Application.streamingAssetsPath }
        };
    }

    [System.Serializable]
    public class SaveWriteArgs : EventArgs
    {
        public string SaveName { get; set; } = string.Empty;
        public virtual int SlotNumber { get; set; } = 0;
        public SaveData SaveData { get; set; }
        public SaveDirectoryType SaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveWriteArgs() { }

    }

    public enum SaveDirectoryType
    {
        Null,
        DataPath,
        PersistentDataPath,
        StreamingAssetsPath,
    }
}