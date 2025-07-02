using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// Handles the interactions with persistent storage (the hard drives PCs have, for example)
    /// for loading and saving game data.
    /// </summary>
    public class FileSaveRepository : ISaveRepository
    {
        public virtual void Init(SaveReader saveReader = null, SaveWriter saveWriter = null)
        {
            this.saveReader = saveReader;
            this.saveWriter = saveWriter;

            AccountForNullInputs();
            void AccountForNullInputs()
            {
                if (saveReader == null)
                {
                    this.saveReader = ScriptableObject.CreateInstance<SaveReader>();
                }

                if (saveWriter == null)
                {
                    this.saveWriter = ScriptableObject.CreateInstance<SaveWriter>();
                }
            }

            PrepRequestCache();
            void PrepRequestCache()
            {
                readRequest = new SaveReadRequest
                {
                    BaseSaveDirectory = SaveDir,
                    SlotNumber = 0 // Default slot number, can be changed later
                };
            }
        }

        protected SaveReader saveReader;
        protected SaveWriter saveWriter;

        protected SaveReadRequest readRequest;
        protected SaveDirectoryType SaveDir { get { return SaveSystem.S.SaveDirectoryType; } }

        public virtual async Task<CompositeSaveData> LoadMainSaveAsync(int slot)
        {
            readRequest.SlotNumber = slot;
            var mainState = await saveReader.ReadMainSaveDataFromDisk(readRequest);
            return mainState;
        }

        public virtual async Task SaveAsync(SaveDataSet saveSet)
        {
            var meta = saveSet.Meta;
            int slot = meta.SlotNumber;

            PrepWriteRequest();
            void PrepWriteRequest()
            {
                writeReq.Clear();
                writeReq.SaveMetaData = meta;
                writeReq.SlotNumber = slot;
                writeReq.BaseSaveDirectory = SaveDir;
                writeReq.MainState = saveSet.MainState;
            }
            
            await saveWriter.WriteOneToDisk(writeReq);
        }

        protected SaveWriteRequest writeReq = new SaveWriteRequest();

        public virtual async Task<ISaveMetaData> LoadMetaDataAsync(int slot)
        {
            readRequest.SlotNumber = slot;
            var meta = await saveReader.ReadMetadataFromDisk(readRequest);
            return meta;
        }

        /// With how fast deletion operations are, it seems we won't need this to be async
        public virtual void Delete(int slot)
        {
            string path = GetPathTo(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted save at slot {slot}, path {path}");
            }
        }

        /// <summary>
        /// Returns the path to the save file for the given slot number.
        /// </summary>
        public virtual string GetPathTo(int slot)
        {
            forPathFinding.SlotNumber = slot;
            string result = FileUtils.GetPathToFile(SaveDir, slot, saveReader);
            return result;
        }

        protected SaveReadRequest forPathFinding = new SaveReadRequest
        {
            BaseSaveDirectory = SaveSystem.S.SaveDirectoryType,
            SlotNumber = 0 // Default slot number, can be changed later
        };
    }

    public interface ISaveRepository
    {
        /// <summary>
        /// Reads save data from file based on theinput, returning said data.
        /// </summary>
        Task<CompositeSaveData> LoadMainSaveAsync(int slot);

        /// <summary>
        /// Reads only the metadata for a given slot number from file.
        /// </summary>
        Task<ISaveMetaData> LoadMetaDataAsync(int slot);    
        Task SaveAsync(SaveDataSet saveSet);
        
        void Delete(int slot);
        string GetPathTo(int slot);
    }


}