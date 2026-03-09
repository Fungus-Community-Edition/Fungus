using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using FullSerializer;
using System.IO;
using System;

namespace AtMycelia.SaveSys
{ 
    public static class SaveSystem
    {
        public static fsSerializer DefaultSerializer { get; } = new fsSerializer();

        public static readonly int minSlotNumber = 1;
        private static bool initted;

        /// <summary>
        /// Whether or not late-time replacement for certain modules is allowed. Things like
        /// the save registry, what with how that handles volatile data.
        /// </summary>
        public static bool CoreLockMode { get; private set; }

        #region Submodules
        // For third-party customizability, we want to give the option to inject the 
        // individual SaveManager dependencies (instead of needing to prep a whole
        // SaveManager themselves, then passing it to this class). Client code might
        // only want to swap out one module of the implementation, after all

        /// <summary>
        /// Handler for saving and loading data to and from persistent storage.
        /// </summary>
        public static ISaveRepository SaveRepo
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

        // We use private gets here to better control access to the modules
        public static SaveRegistry Registry
        {
            private get { return SaveManager.Registry; }
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

        public static SaveLoader Loader
        {
            private get { return SaveManager.Loader; }
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

        public static IMetaFactory MetaFactory
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

        public static IMainStateFactory MainStateFactory
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

        public static ISaveManager SaveManager
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
        private static ISaveManager saveManager;
        #endregion

        #region Submodule-Registration

        /// <summary>
        /// Decides what paths to use for saving and loading.
        /// </summary>
        public static IConfigurableSaveSlotPathResolver SavePathResolver
        {
            get => SaveRepo.PathResolver;
            set => SaveRepo.PathResolver = value;
        }

        public static void RegisterSaveDataAppliersMulti(IList<ISaveDataApplier> toRegister)
        {
            for (int i = 0; i < toRegister.Count; i++)
            {
                ISaveDataApplier elem = toRegister[i];
                RegisterSaveDataApplier(elem);
            }
        }

        public static void RegisterSaveDataApplier(ISaveDataApplier applier)
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

        public static IList<ISaveDataApplier> SaveDataAppliers
        {
            get { return new List<ISaveDataApplier>(saveDataAppliers); }
        }
        private static IList<ISaveDataApplier> saveDataAppliers = new List<ISaveDataApplier>();

        public static void UnregisterSaveDataApplier(ISaveDataApplier applier)
        {
            if (applier == null)
            {
                Debug.LogError("Cannot unregister a null ISaveDataApplier.");
                return;
            }

            saveDataAppliers.Remove(applier);
        }

        public static void ClearSaveDataAppliers()
        {
            saveDataAppliers.Clear();
        }
        #endregion

        #region Save/Load/Delete Operations
        public static Task SaveToSlotAsync(int slotNum)
        {
            return saveManager.SaveToSlotAsync(slotNum);
        }

        public static Task<CompositeSaveData> LoadMainAsync(int slotNum, bool loadScene = true,
            CancellationToken token = default)
        {
            return saveManager.LoadMainAsync(slotNum, loadScene, token);
        }

        public static Task<ISaveMetaData> LoadMeta(int slotNum, CancellationToken token = default)
        {
            return saveManager.LoadMetaAsync(slotNum, token);
        }

        public static void DeleteSave(int slotNum)
        {
            saveManager.DeleteSave(slotNum);    
        }
        #endregion

        public static void Init()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("SaveSystem.Init called in edit mode. This call will be ignored.");
                return;
            }

            if (initted)
            {
                Debug.LogWarning("SaveSystem already initialized. Init call ignored.");
                return;
            }

            initted = true;

            Task.Run(async () =>
            {
                await Task.Delay(500);
                ActivateCoreLockAndInitSaveManager();
            });
            
        }

        private static void ActivateCoreLockAndInitSaveManager()
        {
            CoreLockMode = true;
            saveManager.Init();
            FullyInitted = true;
            CoreLockActivated();
        }

        public static event Action CoreLockActivated = delegate { };
        public static bool FullyInitted { get; private set; }

        public static bool DoesSaveExist(int slotNum)
        {
            return SaveManager.SlotExists(slotNum);
        }

        public static void ResetStaticsForTest()
        {
            initted = false;
            CoreLockMode = false;
            saveManager = null;
            saveDataAppliers = new List<ISaveDataApplier>();
            markerManager = new ProgressMarkerManager();
            FullyInitted = false;
        }

        #region Resolving Details about Paths

        public static SaveDirectoryType SaveDirectoryType { get; set; }

        public static string GetSaveDirectory(SaveDirectoryType dirType)
        {
            string result = SavePathResolver.GetSaveFolderPath(dirType);
            if (!Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        public static string FileExtension => SavePathResolver.FileExtension;

        public static string RelativePath => SavePathResolver.RelativePath;

        public static string NumberFormat => SavePathResolver.NumberFormat;

        public static string GetSaveFilePath(string fileName, object input)
        {
            return SavePathResolver.GetSaveFilePath(fileName, input);
        }

        public static string GetSaveFolderPath(object input)
        {
            return SavePathResolver.GetSaveFolderPath(input);
        }

        public static string GetSaveFilePath(SaveDirectoryType input, int slotNumber)
        {
            return SavePathResolver.GetSaveFilePath(input, slotNumber);
        }

        public static string GetSaveFolderPath(SaveDirectoryType input)
        {
            return SavePathResolver.GetSaveFolderPath(input);
        }

        public static string GetSaveFilePath(string fileName, SaveDirectoryType input)
        {
            return SavePathResolver.GetSaveFilePath(fileName, input);
        }

        public static string GetSaveFileName(int slotNumber)
        {
            return SavePathResolver.GetSaveFileName(slotNumber);
        }

        public static string GetSaveFilePath(object input, int slotNumber)
        {
            return SavePathResolver.GetSaveFilePath(input, slotNumber);
        }
        #endregion

        #region ProgressMarker-Management
        public static void RegisterProgressMarker(string id, int order = 0)
        {
            markerManager.RegisterProgressMarker(id, order);
        }

        private static ProgressMarkerManager markerManager = new ProgressMarkerManager();

        public static void UnregisterProgressMarker(string id)
        {
            markerManager.UnregisterProgressMarker(id);
        }

        public static IList<ProgressMarker> ProgressMarkers
        {
            get { return markerManager.ProgressMarkers; }
        }

        public static ProgressMarker GetProgressMarkerByID(string id)
        {
            return markerManager.GetProgressMarkerByID(id);
        }

        public static void ClearProgressMarkers()
        {
            markerManager.ClearProgressMarkers();
        }

        public static void SetProgressMarkerOrder(string id, int order)
        {
            markerManager.SetProgressMarkerOrder(id, order);
        }

        public static bool IsProgressMarkerRegistered(string id)
        {
            return markerManager.IsProgressMarkerRegistered(id);
        }

        public static IEnumerable<ProgressMarker> GetOrderedMarkers()
        {
            return markerManager.GetOrderedMarkers();
        }

        public static void EnsureMultiMarkersRegistered(IList<ProgressMarker> markers)
        {
            for (int i = 0; i < markers.Count; i++)
            {
                string id = markers[i].Id;
                EnsureMarkerRegistered(id, markers[i].Order);
            }
        }

        public static void EnsureMarkerRegistered(string id, int order = 0)
        {
            if (!IsProgressMarkerRegistered(id))
            {
                RegisterProgressMarker(id, order);
            }
        }
        #endregion
    }

}