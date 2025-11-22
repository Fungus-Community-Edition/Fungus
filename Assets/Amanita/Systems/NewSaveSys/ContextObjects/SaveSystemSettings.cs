using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveSystemSettings : ScriptableObject
    {
        // We have these as ScriptableObject references to reduce persistence headaches. This means
        // that all custom ISaveReader and ISaveWriter implementations must also be ScriptableObjects.
        [SerializeField] protected ScriptableObject _saveReader;
        [SerializeField] protected ScriptableObject _saveWriter;
        [SerializeField] protected SaveStorageSettings _storageSettings;

        public virtual ISaveReader SaveReader
        {
            get => _saveReader as ISaveReader;
            set
            {
                if (value == _saveReader as ISaveReader)
                {
                    return;
                }
                if (value != null && value is not ScriptableObject so)
                {
                    Debug.LogError("SaveReader must be a ScriptableObject.", this);
                    return;
                }
                _saveReader = value as ScriptableObject;
            }
        }

        public virtual ISaveWriter SaveWriter
        {
            get => _saveWriter as ISaveWriter;
            set
            {
                if (value == _saveWriter as ISaveWriter)
                {
                    return;
                }

                if (value != null && value is not ScriptableObject)
                {
                    Debug.LogError("SaveWriter must be a ScriptableObject.", this);
                    return;
                }

                _saveWriter = value as ScriptableObject;
            }
        }

        public SaveStorageSettings StorageSettings
        {
            get => _storageSettings;
            set => _storageSettings = value;
        }
    }
}