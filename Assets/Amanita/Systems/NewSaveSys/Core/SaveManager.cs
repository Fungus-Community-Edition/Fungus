using Amanita.SaveSys.VScripting;
using Amanita.Utils;
using Amanita.VScripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityObj = UnityEngine.Object;

namespace Amanita.SaveSys
{
    public class SaveManager : ISaveManager
    {
        public virtual Task Init()
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

            UnityThreadUtil.RunOnMainThread(() =>
            {
                IList<ISaveMetaData> metasOnDisk = SaveRepo.LoadAllMetasOnDisk();
                for (int i = 0; i < metasOnDisk.Count; i++)
                {
                    ISaveMetaData meta = metasOnDisk[i];
                    SaveDataSet dataSet = new SaveDataSet(meta, null);
                    Registry.AddSave(dataSet);
                }
                SaveSysSignals.SaveMetasReadOnInit(metasOnDisk);
            });

            return Task.CompletedTask;

        }
        public virtual int MaxSlots { get; set; } = 100;

        public Func<Task> AfterSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

        public SaveManager(ISaveRepository saveRepo, SaveRegistry registry,
                        SaveLoader loader, IMetaFactory metaFactory,
                        IMainStateFactory mainStateFactory)
        {
            this.SaveRepo = saveRepo;
            this.Registry = registry;
            this.Loader = loader;
            this.MetaFactory = metaFactory;
            this.MainStateFactory = mainStateFactory;

            BeforeSceneLoadAsync = StopAllExecutingFlowchartBlocks;

            static async Task StopAllExecutingFlowchartBlocks()
            {
                var flowcharts = AmanitaManager.S.FlowchartsInScene;
                for (int i = 0; i < flowcharts.Count; i++)
                {
                    var fc = flowcharts[i];
                    if (fc == null)
                    {
                        continue;
                    }
                    // We only want to stop this flowchart's executing blocks if it is NOT set 
                    // to persist across scenes. Otherwise, stopping its blocks here would
                    // interrupt any ongoing logic that is meant to continue.
                    bool isPersistent = fc.gameObject.scene.name == "DontDestroyOnLoad";
                    if (isPersistent || !fc.HasExecutingBlocks())
                    {
                        continue;
                    }

                    // If any blocks are executing, stop them so the next scene can start its Init
                    Debug.Log($"Stopping all executing blocks in Flowchart named {fc.name} with " +
                        $"GUID {fc.UniqueId} before loading save.");
                    fc.StopAllBlocks();
                }
                await Task.CompletedTask;
            }
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

            string logMessage = $"Save Manager: Saved to slot {slotNum}.";
            Debug.Log(logMessage);
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

            var saveSet = Registry.GetSave(slotNum);
            ISaveMetaData meta = Registry.GetSaveMeta(slotNum);
            CompositeSaveData mainData = Registry.GetMainSave(slotNum) as CompositeSaveData;
            bool loadedMainData = mainData != null;
            if (!loadedMainData)
            {
                // That means we didn't load any main data for that slot yet. This is the time
                // to do so, given how on startup, we only load the metas.
                mainData = await SaveRepo.LoadMainSaveAsync(slotNum, token);
                saveSet.MainState = mainData;
                Registry.AddSave(saveSet);
                string logMessage = $"Save Manager: Loaded main save data for slot {slotNum} into registry.";
                Debug.Log(logMessage);
            }

            Scene sceneToLoad = default;

            await PrepBeforeLoad();
            async Task PrepBeforeLoad()
            {
                sceneToLoad = DecideSceneToLoad();
                Scene DecideSceneToLoad()
                {
                    Scene result = SceneManager.GetSceneByName(meta.SceneName);
                    Debug.Log($"Scene found by name: {result.name}, valid: {result.IsValid()}");
                    if (!result.IsValid())
                    {
                        result = SceneManager.GetSceneByBuildIndex(meta.SceneBuildIndex);
                    }

                    bool shouldLoadScene = loadScene && result.IsValid();
                    if (!shouldLoadScene)
                    {
                        result = SaveSysConstants.DoNotLoad;
                    }
                    return result;
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
                else
                {
                    Debug.Log("Not loading scene as per request.");
                }

                return true;
            }

            if (shouldStopHere)
            {
                return mainData;
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