using System;
using System.IO;
using System.Threading;
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
        public FileSaveRepository(SaveReader saveReader, SaveWriter saveWriter, SaveDirectoryType saveDir)
        {
            Validate(saveReader, saveWriter);
            this.saveReader = saveReader;
            this.saveWriter = saveWriter;
            this.saveDir = saveDir;

            PrepRequestCache();
            void PrepRequestCache()
            {
                readRequest = new SaveReadRequest
                {
                    BaseSaveDirectory = saveDir,
                    SlotNumber = 0 // Default slot number, can be changed later
                };

                writeReq = new SaveWriteRequest();

                forPathFinding = new SaveReadRequest
                {
                    BaseSaveDirectory = this.saveDir,
                    SlotNumber = 0 // Default slot number, can be changed later
                };
            }
        }

        protected virtual void Validate(SaveReader reader, SaveWriter writer)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader), "Reader passed to FileSaveRepository is null.");
            }

            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer), "Writer passed to FileSaveRepository is null.");
            }
        }

        protected SaveReader saveReader;
        protected SaveWriter saveWriter;
        protected SaveDirectoryType saveDir;

        protected SaveReadRequest readRequest;
        protected SaveWriteRequest writeReq;
        protected SaveReadRequest forPathFinding;

        public virtual async Task<CompositeSaveData> LoadMainSaveAsync(int slot, CancellationToken token = default)
        {
            readRequest.SlotNumber = slot;
            var mainState = await saveReader.ReadMainSaveDataFromDisk(readRequest, token);
            return mainState;
        }

        public virtual async Task SaveAsync(SaveDataSet saveSet, CancellationToken token = default)
        {
            var meta = saveSet.Meta;
            int slot = meta.SlotNumber;

            PrepWriteRequest();
            void PrepWriteRequest()
            {
                writeReq.Clear();
                writeReq.SaveMetaData = meta;
                writeReq.SlotNumber = slot;
                writeReq.BaseSaveDirectory = saveDir;
                writeReq.MainState = saveSet.MainState;
            }
            
            await saveWriter.WriteOneToDisk(writeReq, token);
            
        }

        public virtual async Task<ISaveMetaData> LoadMetaDataAsync(int slot, CancellationToken token = default)
        {
            readRequest.SlotNumber = slot;
            var meta = await saveReader.ReadMetadataFromDisk(readRequest, token);
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
            PrepReadRequest();
            void PrepReadRequest()
            {
                forPathFinding.Clear();
                forPathFinding.BaseSaveDirectory = SaveSystem.S.SaveDirectoryType;
                forPathFinding.SlotNumber = slot;
            }

            string result = saveReader.GetSaveFilePath(saveDir, slot);
            return result;
        }

        
    }

    /// <summary>
    /// For handling the interactions with persistent storage for loading and saving game data.
    /// </summary>
    public interface ISaveRepository
    {
        /// <summary>
        /// Loads save data from file based on the input, returning said data.
        /// </summary>
        Task<CompositeSaveData> LoadMainSaveAsync(int slot, CancellationToken token = default);

        /// <summary>
        /// Loads only the metadata for a given slot number from file.
        /// </summary>
        Task<ISaveMetaData> LoadMetaDataAsync(int slot, CancellationToken token = default);    
        Task SaveAsync(SaveDataSet saveSet, CancellationToken token = default);
        
        void Delete(int slot);
        string GetPathTo(int slot);
    }


}