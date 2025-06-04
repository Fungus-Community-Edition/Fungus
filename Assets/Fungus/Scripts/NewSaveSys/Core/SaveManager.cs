using Amanita.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveManager : ISaveManager
    {
        protected const int MaxSlots = 5; // Or make this configurable

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
        
        public virtual void RegisterMultiMainEncoders(IList<IMainSaveCodec> encoders)
        {
            for (int i = 0; i < encoders.Count; i++)
            {
                RegisterMainEncoder(encoders[i]);
            }
        }

        public virtual void RegisterMainEncoder(IMainSaveCodec encoder)
        {
            _mainEncoders.Add(encoder);
        }

        protected IList<IMainSaveCodec> _mainEncoders = new List<IMainSaveCodec>();
        public virtual SaveWriter SaveWriter { get; set; }
        public virtual SaveReader SaveReader { get; set; }

        public virtual async Task SaveTo(int slotNum)
        {
            await Save(slotNum, "");
        }

        public virtual async Task Save(int slotNum, string saveName)
        {
            Validate(slotNum, registerAndWriteOp);
            await Process();
            async Task Process()
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
                            IMainSaveCodec currentEncoder = _mainEncoders[i];
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

                // And now with the current state of the game all nice and recorded...
                PrepWriteRequest();
                void PrepWriteRequest()
                {
                    writeRequest.SaveMetaData = meta;
                    writeRequest.SlotNumber = slotNum;
                    writeRequest.SaveName = saveName;
                    writeRequest.BaseSaveDirectory = SaveDirType;
                    writeRequest.MainState = mainState;
                }

                await Task.Run(() => SaveWriter.WriteOneToDisk(writeRequest));
                
            }
        }

        protected static string registerAndWriteOp = "registerOrWrite";
        

        protected virtual void Validate(int slotNum, string operation)
        {
            if (slotNum < 0)
            {
                string errorMessage = $"Cannot {operation} a save with a negative slot number.";
                throw new ArgumentOutOfRangeException(nameof(slotNum), slotNum, errorMessage);
            }
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

        public virtual async Task<CompositeSaveData> LoadSave(int slotNum)
        {
            Validate(slotNum, loadOp);
            throw new NotImplementedException();
        }

        protected static string loadOp = "load";

        public virtual async Task DeleteSave(int slotNum)
        {
            Validate(slotNum, deleteOp);
            throw new NotImplementedException();
        }

        protected static string deleteOp = "delete";

        public virtual IList<SaveSlot> GetAllSlots()
        {
            // Load all slot files or PlayerPrefs keys, return as list
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

}