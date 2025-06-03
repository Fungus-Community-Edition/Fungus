using Amanita.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface ISaveManager
    {
        Task SaveAsync(int slotNumber);
        Task<CompositeSaveData> LoadAsync(int slot);

        IList<int> GetOccupiedSlots();
        void DeleteSlot(int slot);
        bool SlotExists(int slot);
    }


    public class SaveManager : ISaveManager
    {
        public SaveManager()
        {
            registry = new SaveRegistry();
            serializer = new SaveSerializer();
            loader = new SaveLoader();
        }

        protected SaveRegistry registry;
        protected SaveSerializer serializer;
        protected SaveLoader loader;
        public SaveDirectoryType SaveDirType { get; set; } = SaveDirectoryType.DataPath;
        public virtual string SaveRelativePath { get; set; } = "/Saves";

        public virtual string FullSaveDir
        {
            get
            {
                string baseDir = SaveSystem.SaveDirectoryPaths[SaveDirType];
                string result = Path.Combine(baseDir, SaveRelativePath);
                return result;
            }
        }
        
        public virtual void RegisterMultiMainEncoders(IList<SaveEncoder> encoders)
        {
            for (int i = 0; i < encoders.Count; i++)
            {
                RegisterMainEncoder(encoders[i]);
            }
        }

        public virtual void RegisterMainEncoder(SaveEncoder encoder)
        {
            _mainEncoders.Add(encoder);
        }

        protected IList<SaveEncoder> _mainEncoders = new List<SaveEncoder>();
        public virtual SaveWriter SaveWriter { get; set; }
        public virtual SaveReader SaveReader { get; set; }

        public virtual void RegisterAndWriteSave(int slotNum, string saveName = "")
        {
            CompositeSaveData mainState = CreateMainState();
            CompositeSaveData CreateMainState()
            {
                IList<SaveDataUnit> unitsNeeded = GetUnitsForGameState();
                IList<SaveDataUnit> GetUnitsForGameState()
                {
                    IList<SaveDataUnit> units = new List<SaveDataUnit>();

                    for (int i = 0; i < _mainEncoders.Count; i++)
                    {
                        SaveEncoder currentEncoder = _mainEncoders[i];
                        IList<SaveDataUnit> newUnits = currentEncoder.FindAndEncodeAll();
                        units.AddRange(newUnits);
                    }

                    return units;
                }

                CompositeSaveData mainState = new CompositeSaveData(unitsNeeded);
                return mainState;
            }
            
            SaveMetaData meta = CreateMetaFor(slotNum);

            SaveDataSet newSet = new SaveDataSet(meta, mainState);
            registry.AddSave(newSet);

            PrepWriteRequest();
            void PrepWriteRequest()
            {
                writeRequest.SaveMetaData = meta;
                writeRequest.SlotNumber = slotNum;
                writeRequest.SaveName = saveName;
                writeRequest.BaseSaveDirectory = SaveDirType;
                writeRequest.MainState = mainState;
            }

            SaveWriter.WriteOneToDisk(writeRequest);
        }

        protected virtual SaveMetaData CreateMetaFor(int slot)
        {
            SaveMetaData meta = new SaveMetaData();
            meta.SlotNumber = slot;

            if (!string.IsNullOrEmpty(Application.version))
            {
                meta.SaveVersion = Application.version;
            }

            return meta;
        }

        protected SaveWriteRequest writeRequest = new SaveWriteRequest();

        public virtual void LoadSave(string saveName)
        {
            throw new NotImplementedException();
        }

        public virtual void DeleteSave(string saveName)
        {
            throw new NotImplementedException();
        }

        protected const int MaxSlots = 5; // Or make this configurable

        public virtual List<SaveSlot> GetAllSlots()
        {
            // Load all slot files or PlayerPrefs keys, return as list
            throw new NotImplementedException();
        }

        public virtual void SaveToSlot(int slotIndex, SaveDataUnit[] saveDataItems)
        {
            // Serialize and save to file or PlayerPrefs, include metadata
            throw new NotImplementedException();
        }

        public virtual SaveSlot LoadFromSlot(int slotIndex)
        {
            // Load and deserialize slot data
            throw new System.NotImplementedException();
        }

        public virtual void DeleteSlot(int slotIndex)
        {
            // Remove slot data from storage
            throw new NotImplementedException();
        }

        public virtual IList<int> GetOccupiedSlots()
        {
            return registry.GetOccupiedSlots();
        }

        public virtual bool SlotExists(int slot)
        {
            return registry.HasSaveInSlot(slot);
        }

        public virtual async Task SaveAsync(int slotNum)
        {
            // Register the current game state, then request a write
            
            // Implementation sketch
            //var req = new SaveWriteRequest
            //{
            //    SlotNumber = slot,
            //    MainState = data,
            //    SaveMetaData = meta,
            //    BaseSaveDirectory = _baseDir,
            //    RelativePath = _relPath
            //};
            //await Task.Run(() => _writer.WriteOneToDisk(req));

            throw new NotImplementedException();
        }

        public virtual Task<CompositeSaveData> LoadAsync(int slot)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns (what at least would be) the path to the save of the 
        /// specified slot. This function does not take into account 
        /// whether or not a save with that slot exists; it only
        /// considers hypotheticals.
        /// </summary>
        public virtual string GetPathTo(int slot)
        {
            reqForPathFinding.BaseSaveDirectory = SaveDirType;
            reqForPathFinding.SlotNumber = slot;
            string result = SaveReader.GetSavePath(reqForPathFinding);
            return result;
        }

        protected SaveReadRequest reqForPathFinding = new SaveReadRequest();
        // ^Better to cache this than create a new request every time client code
        // wants to know the path of a save.
    }

    public class SaveRegistrationRequest
    {
        public virtual SaveMetaData SaveMetaData { get; set; }
        public virtual SaveData MainSaveData { get; set; }
        public virtual int SlotNumber { get; set; }
    }

}