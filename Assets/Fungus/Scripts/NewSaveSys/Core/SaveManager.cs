using System.Collections.Generic;

namespace Amanita.SaveSys
{
    public class SaveManager
    {
        private SaveRegistry registry;
        private SaveSerializer serializer;
        private SaveStorage storage;
        private SaveLoader loader;

        public SaveManager()
        {
            registry = new SaveRegistry();
            serializer = new SaveSerializer();
            storage = new SaveStorage();
            loader = new SaveLoader();
        }

        public void RegisterSave(string saveName, SaveData saveData)
        {
            registry.AddSave(saveName);
            var json = serializer.Serialize(saveData);
            storage.WriteSaveFile(saveName, json);
        }

        public void LoadSave(string saveName)
        {
            var json = storage.ReadSaveFile(saveName);
            var saveData = serializer.Deserialize(json);
            loader.ApplySave(saveData);
        }

        public void DeleteSave(string saveName)
        {
            registry.RemoveSave(saveName);
            storage.DeleteSaveFile(saveName);
        }

        private const int MaxSlots = 5; // Or make this configurable

        public List<SaveSlot> GetAllSlots()
        {
            // Load all slot files or PlayerPrefs keys, return as list
            throw new System.NotImplementedException();
        }

        public void SaveToSlot(int slotIndex, SerializedSaveData[] saveDataItems)
        {
            // Serialize and save to file or PlayerPrefs, include metadata
        }

        public SaveSlot LoadFromSlot(int slotIndex)
        {
            // Load and deserialize slot data
            throw new System.NotImplementedException();
        }

        public void DeleteSlot(int slotIndex)
        {
            // Remove slot data from storage
        }
    }

}