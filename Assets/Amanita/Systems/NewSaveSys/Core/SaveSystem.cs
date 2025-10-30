using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using FullSerializer;
using System.Linq;

namespace Amanita.SaveSys
{ 
    public class SaveSystem : MonoBehaviour, ISaveSlotPathResolver<SaveDirectoryType>, IProgressMarkerManager
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

        public static fsSerializer DefaultSerializer { get; } = new fsSerializer();

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

        /// <summary>
        /// Decides what paths to use for saving and loading.
        /// </summary>
        public virtual ISaveSlotPathResolver<SaveDirectoryType> SavePathResolver { get; set; } = new DefaultSavePathResolver();

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

        public virtual string GetSaveDirectory(SaveDirectoryType dirType)
        {
            string result = SavePathResolver.GetSaveFolderPath(dirType);
            return result;
        }
        
        protected static string InaccessibleVarFormat => "Cannot get value of {0}. It's not properly registered yet.";
        protected static string UnmutableVarFormat => "Cannot alter value of {0}. It's not properly registered yet.";

        public string FileExtension => SavePathResolver.FileExtension;

        public string RelativePath => SavePathResolver.RelativePath;

        public string NumberFormat => SavePathResolver.NumberFormat;

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

        public string GetSaveFilePath(string fileName, object input)
        {
            return SavePathResolver.GetSaveFilePath(fileName, input);
        }

        public string GetSaveFolderPath(object input)
        {
            return SavePathResolver.GetSaveFolderPath(input);
        }

        public string GetSaveFilePath(SaveDirectoryType input, int slotNumber)
        {
            return SavePathResolver.GetSaveFilePath(input, slotNumber);
        }

        public string GetSaveFolderPath(SaveDirectoryType input)
        {
            return SavePathResolver.GetSaveFolderPath(input);
        }

        public string GetSaveFilePath(string fileName, SaveDirectoryType input)
        {
            return SavePathResolver.GetSaveFilePath(fileName, input);
        }

        public string GetSaveFileName(int slotNumber)
        {
            return SavePathResolver.GetSaveFileName(slotNumber);
        }

        public string GetSaveFilePath(object input, int slotNumber)
        {
            return SavePathResolver.GetSaveFilePath(input, slotNumber);
        }
    
        public virtual void RegisterProgressMarker(string id, int order = 0)
        {
            markerManager.RegisterProgressMarker(id, order);

        }

        protected ProgressMarkerManager markerManager = new ProgressMarkerManager();

        protected IList<ProgressMarker> progressMarkers = new List<ProgressMarker>();

        public virtual void UnregisterProgressMarker(string id)
        {
            markerManager.UnregisterProgressMarker(id);
        }

        public virtual IList<ProgressMarker> ProgressMarkers
        {
            get { return markerManager.ProgressMarkers; }
        }

        public virtual ProgressMarker GetProgressMarkerByID(string id)
        {
            return markerManager.GetProgressMarkerByID(id);
        }

        public virtual void ClearProgressMarkers()
        {
            markerManager.ClearProgressMarkers();
        }

        public virtual void SetProgressMarkerOrder(string id, int order)
        {
            markerManager.SetProgressMarkerOrder(id, order);
        }


        public virtual bool IsProgressMarkerRegistered(string id)
        {
            return markerManager.IsProgressMarkerRegistered(id);
        }

        public IEnumerable<ProgressMarker> GetOrderedMarkers()
        {
            return markerManager.GetOrderedMarkers();
        }

        public virtual void EnsureMarkerRegistered(string id, int order = 0)
        {
            if (!IsProgressMarkerRegistered(id))
            {
                RegisterProgressMarker(id, order);
            }
        }

    }

    public interface IProgressMarkerManager
    {
        void RegisterProgressMarker(string id, int order = 0);

        /// <summary>
        /// Unregisters a progress marker by its ID. If the attempt was successful, returns true.
        /// Otherwise, false.
        /// </summary>
        void UnregisterProgressMarker(string id);
        IList<ProgressMarker> ProgressMarkers { get; }
        ProgressMarker GetProgressMarkerByID(string id);
        void ClearProgressMarkers();
        void SetProgressMarkerOrder(string id, int order);
        bool IsProgressMarkerRegistered(string id);
        IEnumerable<ProgressMarker> GetOrderedMarkers();
    }

    public class ProgressMarkerManager : IProgressMarkerManager
    {
        // Dictionary for O(1) lookups by ID
        protected readonly Dictionary<string, ProgressMarker> progressMarkers
            = new Dictionary<string, ProgressMarker>();

        public virtual void RegisterProgressMarker(string id, int order = 0)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot register a null or empty ProgressMarker ID.");
                return;
            }

            if (progressMarkers.ContainsKey(id))
            {
                Debug.LogWarning($"ProgressMarker with ID '{id}' is already registered.");
                return;
            }

            progressMarkers[id] = new ProgressMarker(id, order);
        }

        public virtual void UnregisterProgressMarker(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot unregister a null or empty ProgressMarker ID.");
            }
            else
            {
                bool successfullyRemoved = progressMarkers.Remove(id);
                if (!successfullyRemoved)
                {
                    Debug.LogWarning($"No ProgressMarker with ID '{id}' found to unregister.");
                }
            }

        }

        public virtual IList<ProgressMarker> ProgressMarkers
        {
            get { return progressMarkers.Values.ToList(); }
        }

        public virtual ProgressMarker GetProgressMarkerByID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            progressMarkers.TryGetValue(id, out var marker);
            return marker;
        }

        public virtual void ClearProgressMarkers()
        {
            progressMarkers.Clear();
        }

        public virtual void SetProgressMarkerOrder(string id, int newOrder)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogError("Cannot set order for a null or empty ProgressMarker ID.");
                return;
            }

            if (progressMarkers.TryGetValue(id, out var marker))
            {
                marker.Order = newOrder;
            }
            else
            {
                Debug.LogWarning($"No ProgressMarker with ID '{id}' found to set order. Creating new one.");
                RegisterProgressMarker(id, newOrder);
            }
        }

        public virtual bool IsProgressMarkerRegistered(string id)
        {
            return !string.IsNullOrEmpty(id) && progressMarkers.ContainsKey(id);
        }

        // Handy helper for ordered execution
        public virtual IEnumerable<ProgressMarker> GetOrderedMarkers()
        {
            return progressMarkers.Values.OrderBy(elem => elem.Order);
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