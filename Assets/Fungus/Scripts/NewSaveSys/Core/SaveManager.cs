using System.Collections.Generic;
using System;

namespace Amanita.SaveSys
{
    public class SaveManager
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

        public virtual SaveWriter SaveWriter { get; set; }
        public virtual SaveReader SaveReader { get; set; }

        public void RegisterAndWriteSave(SaveWriteRequest writeReq)
        {
            SaveWriter.WriteOneToDisk(writeReq);
        }

        public void LoadSave(string saveName)
        {
            throw new NotImplementedException();
        }

        public void DeleteSave(string saveName)
        {
            throw new NotImplementedException();
        }

        protected const int MaxSlots = 5; // Or make this configurable

        public List<SaveSlot> GetAllSlots()
        {
            // Load all slot files or PlayerPrefs keys, return as list
            throw new NotImplementedException();
        }

        public void SaveToSlot(int slotIndex, SaveDataUnit[] saveDataItems)
        {
            // Serialize and save to file or PlayerPrefs, include metadata
            throw new NotImplementedException();
        }

        public SaveSlot LoadFromSlot(int slotIndex)
        {
            // Load and deserialize slot data
            throw new System.NotImplementedException();
        }

        public void DeleteSlot(int slotIndex)
        {
            // Remove slot data from storage
            throw new NotImplementedException();
        }
    }

    public class SaveRegistrationRequest
    {
        public virtual SaveMetaData SaveMetaData { get; set; }
        public virtual SaveData MainSaveData { get; set; }
        public virtual int SlotNumber { get; set; }
    }

}