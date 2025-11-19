using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveSystemSettings : ScriptableObject
    {
        [SerializeField] protected SaveReader _saveReader;
        [SerializeField] protected SaveWriter _saveWriter;
        
        [SerializeField] protected SaveStorageSettings _storageSettings;

        public virtual SaveReader SaveReader
        {
            get => _saveReader;
            set => _saveReader = value;
        }

        public virtual SaveWriter SaveWriter
        {
            get => _saveWriter;
            set => _saveWriter = value;
        }

        public SaveStorageSettings StorageSettings
        {
            get => _storageSettings;
            set => _storageSettings = value;
        }
    }
}