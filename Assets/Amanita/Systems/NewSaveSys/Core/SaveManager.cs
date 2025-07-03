using Amanita.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Threading;

namespace Amanita.SaveSys
{
    public class SaveManager : ISaveManager
    {
        public virtual int MaxSlots { get; set; } = 100;

        
        public Func<Task> AfterSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

        public virtual IVersionProvider VersionProvider { get; protected set; } = new UnityVersionProvider();
        
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
                string baseDir = SaveSystem.S.SaveDirectoryPaths[SaveDirType];
                string result = Path.Combine(baseDir, SaveRelativePath);
                return result;
            }
        }
        
        public virtual void RegisterMultiMainCodecs(IList<IMainSaveCodec> codecs)
        {
            if (codecs == null || codecs.Count == 0)
            {
                Debug.LogWarning("No main codecs provided to register.");
                return;
            }

            for (int i = 0; i < codecs.Count; i++)
            {
                IMainSaveCodec currentEncoder = codecs[i];
                if (currentEncoder == null)
                {
                    Debug.LogWarning($"Main codec at index {i} is null. Skipping registration.");
                    continue;
                }
                RegisterMainCodec(currentEncoder);
            }
        }

        public virtual void RegisterMainCodec(IMainSaveCodec codec)
        {
            mainCodecs.Add(codec);
            Loader.RegisterMainCodec(codec);
        }

        protected IList<IMainSaveCodec> mainCodecs = new List<IMainSaveCodec>();

        public virtual async Task SaveTo(int slotNum, CancellationToken token = default)
        {
            await Save(slotNum, "", token);
        }

        public virtual async Task Save(int slotNum, string saveName, CancellationToken token = default)
        {
            if (!Validate(slotNum, registerAndWriteOp))
            {
                return;
            }

            await Process();
            async Task Process()
            {
                CompositeSaveData mainState = await MainStateFactory.CreateMainState();


                //CompositeSaveData mainState = CreateMainState();
                //CompositeSaveData CreateMainState()
                //{
                //    IList<SaveDataUnit> unitsNeeded = GetUnitsForGameState();
                //    IList<SaveDataUnit> GetUnitsForGameState()
                //    {
                //        IList<SaveDataUnit> units = new List<SaveDataUnit>();

                //        for (int i = 0; i < mainCodecs.Count; i++)
                //        {
                //            IMainSaveCodec currentEncoder = mainCodecs[i];
                //            IList<SaveDataUnit> newUnits = currentEncoder.FindAndEncodeAll();
                //            units.AddRange(newUnits);
                //        }

                //        return units;
                //    }

                //    CompositeSaveData mainState = new CompositeSaveData(unitsNeeded);
                //    return mainState;
                //}

                ISaveMetaData meta = MetaFactory.CreateMeta(slotNum);

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

            Scene sceneToLoad = default;
            CompositeSaveData mainData = null;
            ISaveMetaData meta = null;

            await BeforeLoadPrep();
            async Task BeforeLoadPrep()
            {
                Task<CompositeSaveData> getMainState = GetMainStateAsync();
                async Task<CompositeSaveData> GetMainStateAsync()
                {
                    if (Registry.HasMainSaveInSlot(slotNum))
                    {
                        return (CompositeSaveData)Registry.GetMainSave(slotNum);
                    }
                    else
                    {
                        return await SaveRepo.LoadMainSaveAsync(slotNum, token);
                    }
                }

                Task<Scene> getSceneToLoad = DecideSceneToLoad();
                async Task<Scene> DecideSceneToLoad()
                {
                    if (Registry.HasSaveInSlot(slotNum))
                    {
                        meta = Registry.GetSaveMeta(slotNum);
                    }
                    else
                    {
                        meta = await LoadMeta(slotNum, token);
                    }
                    Scene sceneToLoad = SceneManager.GetSceneByName(meta.SceneName);
                    if (!sceneToLoad.IsValid())
                    {
                        sceneToLoad = SceneManager.GetSceneByBuildIndex(meta.SceneBuildIndex);
                    }

                    bool shouldLoadScene = loadScene && sceneToLoad.IsValid() && sceneToLoad != default;
                    if (!shouldLoadScene)
                    {
                        sceneToLoad = SaveSysConstants.DoNotLoad;
                    }
                    return sceneToLoad;
                }

                Task beforeSceneLoadHandlerTask = ExecuteHandlers(BeforeSceneLoadAsync);
                await ExecuteHandlers(BeforeSceneLoadAsync);
                await Task.WhenAll(getMainState, getSceneToLoad, beforeSceneLoadHandlerTask);

                sceneToLoad = getSceneToLoad.Result;
                mainData = getMainState.Result;
            }

            bool shouldStopHere = ValidateScene(sceneToLoad) == false;
            bool ValidateScene(Scene scene)
            {
                if (loadScene)
                {
                    if (!sceneToLoad.Equals(SaveSysConstants.DoNotLoad) && !sceneToLoad.IsValid())
                    {
                        string errorMessage = $"Cannot load scene {sceneToLoad.name} because it is not valid. " +
                                              $"Please check the save metadata for slot {slotNum}.";
                        Debug.LogError(errorMessage);
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
            return mainData;
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
        // ^Better to cache this than create a new request every time client code
        // wants to know the path of a save.
    
        public virtual CompositeSaveData GetMainFrom(int slot)
        {
            CompositeSaveData mainData = (CompositeSaveData) Registry.GetMainSave(slot);
            return mainData;
        }

        public virtual void ClearSaveData()
        {
            Registry.Clear();
        }
    }

}