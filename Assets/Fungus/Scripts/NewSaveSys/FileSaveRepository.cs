using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
{
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

        public async Task<ISaveMetaData> LoadMetaDataAsync(int slot)
        {
            readRequest.SlotNumber = slot;
            var meta = await saveReader.ReadMetadataFromDisk(readRequest);
            return meta;
        }

        public Task DeleteAsync(int slot)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Returns the path to the save file for the given slot number.
        /// </summary>
        public string GetPathTo(int slot)
        {
            forPathFinding.SlotNumber = slot;
            string result = saveReader.GetSavePath(forPathFinding);
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
        Task<CompositeSaveData> LoadMainSaveAsync(int slot);
        Task<ISaveMetaData> LoadMetaDataAsync(int slot);    
        Task SaveAsync(SaveDataSet saveSet);
        
        Task DeleteAsync(int slot);
        string GetPathTo(int slot);
    }


}