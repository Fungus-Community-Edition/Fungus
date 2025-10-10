using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{ 
    public class SaveSystem : MonoBehaviour
    {
        protected virtual void Awake()
        {
            // It's possible that we might not have an installer to handle this instance, so...
            bool thisIsDuplicateInstance = !initted && _s != null && _s != this;
            if (thisIsDuplicateInstance)
            {
                Debug.LogWarning("SaveSystem already exists. Destroying the new one.");
                // We expect the AmanitaManager to handle the destruction here
                return;
            }
        }

        protected bool initted;

        public virtual async void Init()
        {
            if (S != null && S != this)
            {
                Debug.LogWarning("SaveSystem already exists. Destroying the new one.");
                // We expect the AmanitaManager to handle the destruction here
                return;
            }

            S = this;

            initted = true;

            await Task.Delay(coreLockDelay);
            CoreLockMode = true;
            
        }

        // We expect an instance of this to be attached to the AmanitaManager singleton
        public static SaveSystem S
        {
            get { return _s; }
            set
            {
                _s = value;
            }
        }
        protected static SaveSystem _s;

        protected int coreLockDelay = 1000; // In milliseconds

        /// <summary>
        /// Whether or not late-time replacement for certain modules is allowed. Things like
        /// the save registry, what with how that handles volatile data.
        /// </summary>
        protected virtual bool CoreLockMode { get; set; }

        // For third-party customizability, we want to give the option to inject the 
        // individual SaveManager dependencies (instead of needing to prep a whole
        // SaveManager themselves, then passing it to this class). Client code might
        // only want to swap out one module of the implementation, after all

        public virtual ISaveRepository SaveRepo
        {
            get
            {
                if (SaveManager == null)
                {
                    string warningMessage = "Cannot get save repo when there is no SaveManager registered.";
                    Debug.LogWarning(warningMessage);
                    return null;
                }
                return SaveManager.SaveRepo;
            }
            set
            {
                string warningMessage;
                if (CoreLockMode)
                {
                    warningMessage = "Cannot set SaveRepo of SaveSystem. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                if (SaveManager == null)
                {
                    warningMessage = "Cannot set save repo when there is no SaveManager registered.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                if (SaveManager.SaveRepo == null || !CoreLockMode)
                {
                    SaveManager.SaveRepo = value;
                }

            }
        }

        // We use protected gets here to better control access to the modules
        public virtual SaveRegistry Registry
        {
            protected get { return SaveManager.Registry; }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set Save Registry. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                SaveManager.Registry = value;
            }
        }

        protected SaveRegistry registry;

        public virtual SaveLoader Loader
        {
            protected get { return SaveManager.Loader; }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set save loader. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                SaveManager.Loader = value;
            }
        }

        public virtual IMetaFactory MetaFactory
        {
            get { return SaveManager.MetaFactory; }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set meta factory. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                SaveManager.MetaFactory = value;
            }
        }

        public virtual IMainStateFactory MainStateFactory
        {
            get { return SaveManager.MainStateFactory; }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set main state factory. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                SaveManager.MainStateFactory = value;
            }
        }

        public virtual ISaveManager SaveManager
        {
            get { return saveManager; }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set Save Manager. CoreLockMode is active.";
                    Debug.LogWarning(warningMessage);
                    return;
                }

                saveManager = value;
            }
        }
        protected ISaveManager saveManager;

        public virtual SaveDirectoryType SaveDirectoryType { get; set; }

        public virtual void RegisterMultiMainCodecs(IList<IMainSaveCodec> codecs)
        {
            SaveManager.RegisterMultiMainCodecs(codecs);
        }

        public virtual void RegisterMainCodec(IMainSaveCodec codec)
        {
            SaveManager.RegisterMainCodec(codec);
        }

        public virtual void RegisterSaveDataAppliersMulti(IList<ISaveDataApplier> toRegister)
        {
            for (int i = 0; i < toRegister.Count; i++)
            {
                ISaveDataApplier elem = toRegister[i];
                RegisterSaveDataApplier(elem);
            }
        }

        public virtual void RegisterSaveDataApplier(ISaveDataApplier applier)
        {
            if (applier == null)
            {
                Debug.LogError("Cannot register a null ISaveDataApplier.");
                return;
            }

            if (!saveDataAppliers.Contains(applier))
            {
                saveDataAppliers.Add(applier);
            }
        }

        public virtual IList<ISaveDataApplier> SaveDataAppliers
        {
            get { return new List<ISaveDataApplier>(saveDataAppliers); }
        }
        protected IList<ISaveDataApplier> saveDataAppliers = new List<ISaveDataApplier>();

        public virtual void UnregisterSaveDataApplier(ISaveDataApplier applier)
        {
            if (applier == null)
            {
                Debug.LogError("Cannot unregister a null ISaveDataApplier.");
                return;
            }

            saveDataAppliers.Remove(applier);
        }

        public virtual void ClearSaveDataAppliers()
        {
            saveDataAppliers.Clear();
        }

        public virtual Task SaveTo(int slotNum)
        {
            return saveManager.SaveTo(slotNum);
        }

        public virtual Task<CompositeSaveData> LoadMain(int slotNum, bool loadScene = true,
            CancellationToken token = default)
        {
            return saveManager.LoadMain(slotNum, loadScene, token);
        }

        public virtual Task<ISaveMetaData> LoadMeta(int slotNum, CancellationToken token = default)
        {
            return saveManager.LoadMeta(slotNum, token);
        }

        public virtual void DeleteSave(int slotNum)
        {
            saveManager.DeleteSave(slotNum);    
        }

        /// <summary>
        /// CoreLock applies. Getter returns a copy.
        /// </summary>
        public virtual IDictionary<SaveDirectoryType, string> SaveDirectoryPaths
        {
            get
            {
                return new Dictionary<SaveDirectoryType, string>(saveDirectoryPaths);
                // ^We don't want to allow directly changing the contents
            }
            set
            {
                if (CoreLockMode)
                {
                    string warningMessage = "Cannot set save directory paths on module lock.";
                    Debug.Log(warningMessage);
                    return;
                }

                saveDirectoryPaths = value;
            }
        }

        protected IDictionary<SaveDirectoryType, string> saveDirectoryPaths;

        /// <summary>
        /// CoreLock applies.
        /// </summary>
        public virtual void SetSaveDirPath(SaveDirectoryType saveDirectoryType, string path)
        {
            if (CoreLockMode)
            {
                string warningMessage = "Cannot set save directory paths during CoreLockMode.";
                Debug.Log(warningMessage);
                return;
            }

            if (saveDirectoryPaths.ContainsKey(saveDirectoryType))
            {
                saveDirectoryPaths[saveDirectoryType] = path;
            }
            else
            {
                saveDirectoryPaths.Add(saveDirectoryType, path);
            }
        }

        protected static string InaccessibleVarFormat => "Cannot get value of {0}. It's not properly registered yet.";
        protected static string UnmutableVarFormat => "Cannot alter value of {0}. It's not properly registered yet.";

        public static void ResetStaticsForTest()
        {
            S = null;
        }

        protected virtual void OnDestroy()
        {
            if (S == this)
            {
                S = null;
            }
        }

    }

    public enum SaveDirectoryType
    {
        Null,
        DataPath, // Same folder as the exe, apk, etc
        PersistentDataPath, // OS-dependent folder. Overall safest option
        InTheBalls // Semantically the same as DataPath
    }
}