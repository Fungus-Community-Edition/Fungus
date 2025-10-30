using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;
using Amanita.SaveSys.VScripting;
using UnityObj = UnityEngine.Object;

namespace Amanita.SaveSys
{
    public class SaveManager : ISaveManager
    {
        public virtual int MaxSlots { get; set; } = 100;

        public Func<Task> AfterSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

        public virtual IVersionProvider VersionProvider { get; protected set; }

        public SaveManager(ISaveRepository saveRepo, SaveRegistry registry,
                        SaveLoader loader, IMetaFactory metaFactory,
                        IMainStateFactory mainStateFactory)
        {
            this.SaveRepo = saveRepo;
            this.Registry = registry;
            this.Loader = loader;
            this.MetaFactory = metaFactory;
            this.MainStateFactory = mainStateFactory;
        }

        public virtual ISaveRepository SaveRepo { get; set; }
        public virtual SaveRegistry Registry { get; set; }
        public virtual SaveLoader Loader { get; set; }
        public virtual IMetaFactory MetaFactory { get; set; }
        public SaveDirectoryType SaveDirType { get; set; } = SaveDirectoryType.DataPath;
        public virtual string SaveRelativePath { get; set; } = "/Saves";

        public virtual string FullSaveDir
        {
            get
            {
                string baseDir = SaveSystem.S.GetSaveDirectory(SaveDirType);
                string result = Path.Combine(baseDir, SaveRelativePath);
                return result;
            }
        }

        // Transitional API: codecs no longer needed when saving/loading main data
        [Obsolete("Codecs are no longer used for main saves. This method is a no-op.")]
        public virtual void RegisterMultiMainCodecs(IList<IMainSaveCodec> codecs)
        {
            // Intentionally no-op to keep backward compatibility with calling sites
            if (codecs == null || codecs.Count == 0)
            {
                return;
            }
        }

        [Obsolete("Codecs are no longer used for main saves. This method is a no-op.")]
        public virtual void RegisterMainCodec(IMainSaveCodec codec)
        {
            // Intentionally no-op to keep backward compatibility with calling sites
        }

        public virtual async Task SaveTo(int slotNum, CancellationToken token = default)
        {
            await SaveTo(slotNum, "", token);
        }

        public virtual async Task SaveTo(int slotNum, string saveName, CancellationToken token = default)
        {
            if (!Validate(slotNum, registerAndWriteOp))
            {
                return;
            }

            await Process();
            async Task Process()
            {
                CompositeSaveData mainState = await MainStateFactory.CreateMainState();
                ISaveMetaData meta = MetaFactory.CreateMeta(slotNum);
                meta.SaveName = saveName;

                SaveDataSet newSet = new SaveDataSet(meta, mainState);
                Registry.AddSave(newSet);

                await SaveRepo.SaveAsync(newSet, token);
            }
        }

        public virtual IMainStateFactory MainStateFactory { get; set; }
        protected static string registerAndWriteOp = "register or write";

        protected virtual bool Validate(int slotNum, string operation)
        {
            bool result;
            if (slotNum < 0)
            {
                string errorMessage = $"Cannot {operation} a save with a negative slot number.";
                Debug.LogWarning(errorMessage);
                result = false;
            }
            else
            {
                result = true;
            }

            return result;
        }

        protected SaveWriteRequest writeRequest = new SaveWriteRequest();

        /// <summary>
        /// Loads the main save data from the specified slot, getting its state applied to the game.
        /// If loadScene is true, this will load the scene specified in the save metadata.
        /// </summary>
        public virtual async Task<CompositeSaveData> LoadMain(int slotNum,
            bool loadScene = true, CancellationToken token = default)
        {
            if (!Validate(slotNum, loadOp))
            {
                return null;
            }

            if (!Registry.HasMainSaveInSlot(slotNum))
            {
                string errorMessage = $"Cannot load main in slot {slotNum}. No main data is assigned to it.";
                Debug.LogWarning(errorMessage);
                return null;
            }

            Scene sceneToLoad = default;
            CompositeSaveData mainData = (CompositeSaveData)Registry.GetMainSave(slotNum);
            ISaveMetaData meta = Registry.GetSaveMeta(slotNum);

            await PrepBeforeLoad();
            async Task PrepBeforeLoad()
            {
                Scene sceneToLoad = DecideSceneToLoad();
                Scene DecideSceneToLoad()
                {
                    Scene sceneToLoad = SceneManager.GetSceneByName(meta.SceneName);
                    if (!sceneToLoad.IsValid())
                    {
                        sceneToLoad = SceneManager.GetSceneByBuildIndex(meta.SceneBuildIndex);
                    }

                    bool shouldLoadScene = loadScene && sceneToLoad.IsValid();
                    if (!shouldLoadScene)
                    {
                        sceneToLoad = SaveSysConstants.DoNotLoad;
                    }
                    return sceneToLoad;
                }

                Task beforeSceneLoadHandlerTask = ExecuteHandlers(BeforeSceneLoadAsync);
                await beforeSceneLoadHandlerTask;

            }

            bool shouldStopHere = ValidateScene(sceneToLoad) == false;
            bool ValidateScene(Scene scene)
            {
                if (loadScene)
                {
                    if (!sceneToLoad.Equals(SaveSysConstants.DoNotLoad) && !sceneToLoad.IsValid())
                    {
                        string warningMessage = $"No valid scene found for meta: name = {meta.SceneName}, index = {meta.SceneBuildIndex}";
                        Debug.LogWarning(warningMessage);
                        return false;
                    }
                }

                return true;
            }

            if (shouldStopHere)
            {
                return null;
            }

            await Loader.LoadMain(mainData, sceneToLoad);

            await ExecuteHandlers(AfterSceneLoadAsync);

            ExecuteSaveLoadedHandlers();
            static void ExecuteSaveLoadedHandlers()
            {
                SaveSystem saveSys = SaveSystem.S;
                var registeredMarkers = saveSys.ProgressMarkers;
                List<SaveLoadedEvent> saveLoadedHandlers = UnityObj.FindObjectsByType<SaveLoadedEvent>(FindObjectsSortMode.None).ToList();
                // We only want to count the handlers that are either:
                // - set to respond to any save load
                // - set to respond to at least one marker that is registered in the SaveSystem
                saveLoadedHandlers = saveLoadedHandlers
                    .Where(handler => handler.RespondToAny || 
                    handler.MarkerIDs.Any(markerElem => saveSys.IsProgressMarkerRegistered(markerElem)))
                    .ToList();

                // Sort them by the lowest order var of the IDs the handlers go by
                saveLoadedHandlers.Sort(SortSaveLoadedHandlers);

                for (int i = 0; i < saveLoadedHandlers.Count; i++)
                {
                    saveLoadedHandlers[i].ExecuteBlock();
                }
            }

            return mainData;
        }

        protected static int SortSaveLoadedHandlers(SaveLoadedEvent first, SaveLoadedEvent second)
        {
            int lowestOfA = int.MaxValue, lowestOfB = int.MaxValue;

            for (int i = 0; i < first.MarkerIDs.Count; i++)
            {
                string currentId = first.MarkerIDs[i];
                ProgressMarker marker = SaveSystem.S.GetProgressMarkerByID(currentId);
                if (marker != null && marker.Order < lowestOfA)
                {
                    lowestOfA = marker.Order;
                }
            }

            for (int i = 0; i < second.MarkerIDs.Count; i++)
            {
                string currentId = second.MarkerIDs[i];
                ProgressMarker marker = SaveSystem.S.GetProgressMarkerByID(currentId);
                if (marker != null && marker.Order < lowestOfB)
                {
                    lowestOfB = marker.Order;
                }
            }

            return lowestOfA.CompareTo(lowestOfB);
        }

        public Func<Task> BeforeSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };
        protected static string loadOp = "load";

        protected static async Task ExecuteHandlers(Func<Task> hasHandlers, CancellationToken token = default)
        {
            var invocationList = hasHandlers.GetInvocationList();

            foreach (var handler in invocationList.Cast<Func<Task>>())
            {
                await handler();
            }
        }

        public virtual async Task<ISaveMetaData> LoadMeta(int slotNum, CancellationToken token = default)
        {
            if (!Validate(slotNum, loadOp))
            {
                return null;
            }
            ISaveMetaData meta = await SaveRepo.LoadMetaDataAsync(slotNum);
            return meta;
        }

        public virtual void DeleteSave(int slotNum)
        {
            if (slotNum < 0)
            {
                string errorMessage = $"Cannot delete a save with a negative slot number.";
                Debug.LogWarning(errorMessage);
                return;
            }

            if (!SlotExists(slotNum))
            {
                string warningMessage = $"Cannot delete save in slot {slotNum} because it does not exist.";
                Debug.LogWarning(warningMessage);
                return;
            }

            SaveRepo.Delete(slotNum);
            Registry.RemoveSave(slotNum);
        }

        protected static string deleteOp = "delete";

        public virtual IList<SaveDataSet> GetAllSlots()
        {
            return Registry.GetAllSaves();
        }

        public virtual IList<int> GetOccupiedSlots()
        {
            return Registry.GetOccupiedSlots();
        }

        public virtual bool SlotExists(int slot)
        {
            return Registry.HasSaveInSlot(slot);
        }

        /// <summary>
        /// Returns (what at least would be) the path to the save of the 
        /// specified slot. This function does not take into account 
        /// whether or not a save with that slot exists; it only
        /// considers hypotheticals.
        /// </summary>
        public virtual string GetPathTo(int slot)
        {
            string result = SaveRepo.GetPathTo(slot);
            return result;
        }

        protected SaveReadRequest reqForPathFinding = new SaveReadRequest();

        public virtual CompositeSaveData GetMainFrom(int slot)
        {
            CompositeSaveData mainData = (CompositeSaveData)Registry.GetMainSave(slot);
            return mainData;
        }

        public virtual void ClearSaveData()
        {
            Registry.Clear();
        }

        public virtual void SetSaveNameFor(int slot, string newSaveName)
        {
            Registry.SetSaveNameFor(slot, newSaveName);
        }
    }

}