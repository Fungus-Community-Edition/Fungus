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
        public virtual async Task Init()
        {
            EnsureSaveFolderIsThere();
            void EnsureSaveFolderIsThere()
            {
                var resolver = SaveRepo.PathResolver;
                string folderDir = resolver.GetSaveFolderPath(SaveSystem.S.SaveDirectoryType);
                if (!Directory.Exists(folderDir))
                {
                    Directory.CreateDirectory(folderDir);
                }
            }

            await ReadMetasOnDisk();
            async Task ReadMetasOnDisk()
            {
                IList<ISaveMetaData> metasOnDisk = await SaveRepo.LoadAllMetasOnDisk();
                for (int i = 0; i < metasOnDisk.Count; i++)
                {
                    ISaveMetaData meta = metasOnDisk[i];
                    SaveDataSet dataSet = new SaveDataSet(meta, null);
                    Registry.AddSave(dataSet);
                }
                SaveSysSignals.SaveMetasReadOnInit(metasOnDisk);
            }
        }
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
            void ExecuteSaveLoadedHandlers()
            {
                SaveSystem saveSys = SaveSystem.S;
                var registeredMarkers = saveSys.ProgressMarkers.Select((elem) => elem.Id).ToList();

                // We only want to count the handlers that are either:
                // - set to respond to any save load
                // - set to respond to at least one marker that is registered in the SaveSystem
                List<SaveLoadedEvent> saveLoadedHandlers = UnityObj
                .FindObjectsByType<SaveLoadedEvent>(FindObjectsSortMode.None)
                .Where(handler => handler.IsAbleToRespond)
                .ToList();

                Sort(saveLoadedHandlers);

                for (int i = 0; i < saveLoadedHandlers.Count; i++)
                {
                    var handler = saveLoadedHandlers[i];
                    handler.ExecuteBlock();
                }
            }

            return mainData;
        }

        protected virtual void Sort(List<SaveLoadedEvent> toSort)
        {
            SaveSystem saveSys = SaveSystem.S;

            // To save clock cycles, precompute orders
            var handlerOrders = new Dictionary<SaveLoadedEvent, int>(toSort.Count);
            foreach (var handler in toSort)
            {
                handlerOrders[handler] = handler.LowestOrder();
            }

            toSort.Sort((first, second) =>
            {
                int firstOrder = handlerOrders[first];
                int secondOrder = handlerOrders[second];

                bool shouldUseFallback = firstOrder == secondOrder;
                if (shouldUseFallback)
                {
                    int firstId = first.GetInstanceID();
                    int secondId = second.GetInstanceID();
                    return firstId.CompareTo(secondId);
                }

                return firstOrder.CompareTo(secondOrder);
            });
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

        public virtual IList<int> GetOccupiedSlots()
        {
            return Registry.GetOccupiedSlots();
        }

        public virtual bool SlotExists(int slot)
        {
            return Registry.HasSaveInSlot(slot);
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