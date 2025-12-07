using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveSystemSettings : ScriptableObject
    {
        // We have these as ScriptableObject references to reduce persistence headaches. This means
        // that all custom ISaveReader and ISaveWriter implementations must also be ScriptableObjects.
        [SerializeField] private ScriptableObject _saveReader;
        [SerializeField] private ScriptableObject _saveWriter;
        [SerializeField] private SaveStorageSettings _storageSettings;
        [SerializeField] private List<ScriptableObject> mainAppliers = new List<ScriptableObject>() { };
        [SerializeField] private List<ScriptableObject> mainCodecs = new List<ScriptableObject>() { };

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

        /// <summary>
        /// The appliers that will apply the main save data to the game state.
        /// All of these need to be ScriptableObjects, ideally in the form
        /// of project assets.
        /// </summary>
        public virtual IList<ISaveDataApplier> MainAppliers
        {
            get
            {
                List<ISaveDataApplier> appliers = mainAppliers
                    .ConvertAll(so => so as ISaveDataApplier)
                    .FindAll(applier => applier != null);
                return appliers;
            }
            set
            {
                mainAppliers.Clear();
                foreach (var applier in value)
                {
                    if (applier is ScriptableObject so)
                    {
                        mainAppliers.Add(so);
                    }
                    else
                    {
                        Debug.LogError("All MainAppliers must be ScriptableObjects.", this);
                    }
                }
            }
        }
        
        public virtual IList<IMainSaveCodec> MainCodecs
        {
            get
            {
                List<IMainSaveCodec> codecs = mainCodecs
                    .ConvertAll(so => so as IMainSaveCodec)
                    .FindAll(codec => codec != null);
                return codecs;
            }
            set
            {
                mainCodecs.Clear();
                foreach (var codec in value)
                {
                    if (codec is ScriptableObject so)
                    {
                        mainCodecs.Add(so);
                    }
                    else
                    {
                        Debug.LogError("All MainSaveCodecs must be ScriptableObjects.", this);
                    }
                }
            }
        }

        public virtual void AddMainApplier(ISaveDataApplier applier)
        {
            if (applier is ScriptableObject so)
            {
                mainAppliers.Add(so);
            }
            else
            {
                Debug.LogError("MainApplier must be a ScriptableObject.", this);
            }
        }

        public virtual void AddMainCodec(IMainSaveCodec codec)
        {
            if (codec is ScriptableObject so)
            {
                mainCodecs.Add(so);
            }
            else
            {
                Debug.LogError("MainSaveCodec must be a ScriptableObject.", this);
            }
        }

        public virtual void SetMainApplierAtIndex(ISaveDataApplier applier, int index)
        {
            #region Validation
            if (applier is not ScriptableObject so)
            {
                Debug.LogError("MainApplier must be a ScriptableObject.", this);
                return;
            }

            if (index < 0 || index >= mainAppliers.Count)
            {
                Debug.LogError("Index out of range when setting MainApplier.", this);
                return;
            }
            #endregion

            mainAppliers[index] = so;
        }

        public virtual void SetMainCodecAtIndex(IMainSaveCodec codec, int index)
        {
            #region Validation
            if (codec is not ScriptableObject so)
            {
                Debug.LogError("MainSaveCodec must be a ScriptableObject.", this);
                return;
            }
            if (index < 0 || index >= mainCodecs.Count)
            {
                Debug.LogError("Index out of range when setting MainSaveCodec.", this);
                return;
            }
            #endregion
            mainCodecs[index] = so;
        }

        public virtual void RemoveMainApplier(ISaveDataApplier applier)
        {
            mainAppliers.Remove(applier as ScriptableObject);
        }

        public virtual void RemoveMainCodec(IMainSaveCodec codec)
        {
            mainCodecs.Remove(codec as ScriptableObject);
        }
    }
}