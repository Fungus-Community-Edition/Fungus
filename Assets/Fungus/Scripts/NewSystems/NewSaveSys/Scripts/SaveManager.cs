using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Amanita.SaveSys
{
    /// <summary>
    /// The main interface for creating, loading, and deleting saves.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        // Most of the functionality here is passed off to the following submodules. Much of what 
        // this does without passing the job is react to that the submodules do, hence the 
        // stuff in the event-listener region.
        [SerializeField] protected SaveWriter saveWriter;
        [SerializeField] protected SaveReader saveReader;
        
        protected virtual void Awake()
        {
            if (S != null && S != this)
            {
                Destroy(this.gameObject);
                return;
            }

            S = this;

            // Get the necessary components
            gameLoader = GetComponentInChildren<GameLoader>();
            gameSaver = GetComponentInChildren<GameSaver>();

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);
        }

        public static SaveManager S { get; private set; }

        protected GameLoader gameLoader;
        protected GameSaver gameSaver;

        public virtual string SaveDirectory
        {
            get
            {
                // Platform-neutrality
                string dataPath = null;

#if (UNITY_STANDALONE)
                dataPath = Application.dataPath;
#else
                dataPath = Application.persistentDataPath;
#endif

                return Path.Combine(dataPath, "saveData");
            }
        }

        protected virtual void OnEnable()
        {
            ListenForEvents();
        }

        // When it comes to saves being read or written, this manager only cares when it's the specified
        // save readers and writers doing it.
        protected virtual void ListenForEvents()
        {
            saveWriter.GameSaveWritten += OnGameSaveWritten;
            saveReader.GameSaveRead += OnGameSaveRead;
            Signals.GameSaveErased += OnGameSaveErased;
        }

        protected virtual void OnGameSaveWritten(GameSaveData saveData, string filePath, string fileName)
        {
            writtenSavesByFilePaths[filePath] = saveData;
        }

        protected IDictionary<string, GameSaveData> writtenSavesByFilePaths = new Dictionary<string, GameSaveData>();
        public virtual IList<GameSaveData> WrittenSaves
        {
            get
            {
                IList<GameSaveData> result = new List<GameSaveData>(writtenSavesByFilePaths.Values);
                return result;
            }
        }
        protected virtual void OnGameSaveRead(GameSaveData saveData, string filePath, string fileName)
        {
            RegisterSave(saveData, fileName);
        }

        protected IList<GameSaveData> gameSaves = new List<GameSaveData>();

        protected virtual void RegisterSave(GameSaveData saveData, string fileName)
        {
            if (!gameSaves.Contains(saveData))
            {
                gameSaves.Add(saveData);
                gameSavesByFileName[fileName] = saveData;
            }
        }

        protected IDictionary<string, GameSaveData> gameSavesByFileName = 
            new Dictionary<string, GameSaveData>();
        
        protected virtual void OnGameSaveErased(GameSaveData saveData, string filePath, string fileName)
        {
            gameSaves.Remove(saveData);
            gameSavesByFileName.Remove(filePath);
        }

        protected virtual void Start()
        {
            // So other objects (like the SaveSlotManager) can be ready to listen
            // for the save-reading
            if (gameSavesByFileName.Count == 0)
                saveReader.ReadAllFromDisk(SaveDirectory);
        }

        protected virtual void OnDisable()
        {
            UnlistenForEvents();
        }

        /// <summary>
        /// Writes the passed save data to disk.
        /// </summary>
        public virtual void WriteSaveToDisk(GameSaveData saveData)
        {
            saveWriter.WriteOneToDisk(saveData, SaveDirectory);
        }

        /// <summary>
        /// Creates and registers new save data with the passed slot's number, then writing it to disk
        /// if set to do so. Save replacement may happen depending on the aforementioned number.
        /// </summary>
        public virtual bool AddSave(SaveSlot slot)
        {
            if (slot == null)
                throw new System.NullReferenceException("Cannot register a save with a null slot's number.");

            return AddSave(slot.Number);
        }

        /// <summary>
        /// Creates and registers new save data with the passed slot number, then writing it to disk
        /// if set to do so. Save replacement may happen depending on the aforementioned number.
        /// </summary>
        public virtual bool AddSave(int slotNumber)
        {
            var newSaveData = gameSaver.CreateSave(slotNumber);
            return AddSave(newSaveData);
        }

        /// <summary>
        /// Adds a save to the manager. If the passed save shares a number with one it's already
        /// keeping track of, the old one is replaced with the new one.
        /// In which case, the new one will be written to disk regardless of the 
        /// second argument.
        /// </summary>
        public virtual bool AddSave(GameSaveData newSave)
        {
            if (newSave == null)
                return false;

            // See if any save-replacing will happen
            var saveWasReplaced = false;

            for (int i = 0; i < gameSaves.Count; i++)
            {
                var oldSave = gameSaves[i];
                if (oldSave.SlotNumber == newSave.SlotNumber) // Yes, it will!
                {
                    ReplaceSave(oldSave, newSave);
                    saveWasReplaced = true;
                    break;
                }
            }

            if (!saveWasReplaced) // Register it normally.
            {
                
                WriteSaveToDisk(newSave);
            }

            return true;
        }

        /// <summary>
        /// Both saves are assumed to have the same slot number.
        /// </summary>
        protected virtual void ReplaceSave(GameSaveData oldSave, GameSaveData newSave)
        {
            gameSaves.Remove(oldSave);
            gameSaves.Add(newSave);

            var writeNewSave = false;

        }


        /// <summary>
        /// Erases the save data with the passed slot number from disk.
        /// </summary>
        public virtual bool EraseSave(int slotNumber)
        {
            for (int i = 0; i < gameSaves.Count; i++)
                if (gameSaves[i].SlotNumber == slotNumber)
                    return EraseSave(gameSaves[i]);

            return false;

        }

        /// <summary>
        /// Erases the save data linked to the passed slot.
        /// </summary>
        public virtual bool EraseSave(SaveSlot slot)
        {
            if (slot == null)
            {
                Debug.Log("Cannot erase save of a null slot.");
                return false;
            }

            var saveData = slot.SaveData;

            if (saveData == null)
            {
                Debug.Log(slot.name + " has no save data to delete.");
                return false;
            }

            return EraseSave(saveData);
        }

        /// <summary>
        /// Erases the file the passed save data was written to from disk.
        /// </summary>
        public virtual bool EraseSave(GameSaveData saveData)
        {
            // Get the file name associated the save data was written into, and use that to delete 
            // it from the save directory.
            // Using foreach because key-value collections are unindexable.
            var eraseSuccessful = false;

            foreach (var fileName in writtenSavesByFilePaths.Keys)
            {
                var metaFileName = fileName + ".meta";

                if (writtenSavesByFilePaths[fileName] == saveData)
                {
                    var filePath = Path.Combine(SaveDirectory, fileName);
                    var metaFilePath = Path.Combine(SaveDirectory, metaFileName);
                    File.Delete(filePath);
                    File.Delete(metaFilePath);
                    eraseSuccessful = true;
                    Signals.GameSaveErased.Invoke(saveData, filePath, fileName);
                    break;
                }
            }

            return eraseSuccessful;
        }

        // Ultimately, the loading is always passed off to the GameLoader.

        /// <summary>
        /// Loads a save with the passed slot number. Returns true if successful, false otherwise.
        /// </summary>
        public virtual bool LoadSave(int slotNumber)
        {
            for (int i = 0; i < gameSaves.Count; i++)
            {
                var save = gameSaves[i];
                if (save.SlotNumber == slotNumber)
                    return LoadSave(save);
            }

            return false;
        }

        /// <summary>
        /// Loads the save data assigned to the passed slot. Returns true if successful, false
        /// otherwise.
        /// </summary>
        public virtual bool LoadSave(SaveSlot slot)
        {
            // Validate input.
            if (slot == null)
                throw new System.NullReferenceException("Cannot load save data from a null slot.");

            if (slot.SaveData == null)
            {
                Debug.LogWarning("Cannot load save data from " + slot.name + "; it has no save data assigned to it.");
                return false;
            }

            return LoadSave(slot.SaveData);
        }

        /// <summary>
        /// Loads the passed GameSaveData, regardless of whether this manager is keeping track of it
        /// or not.
        /// </summary>
        /// <returns></returns>
        public virtual bool LoadSave(GameSaveData saveData)
        {
            return gameLoader.Load(saveData);
        }

        /// <summary>
        /// Returns the save that has the passed slot number, if it exists. Returns null if
        /// it doesn't.
        /// </summary>
        public virtual GameSaveData GetSave(int slotNumber)
        {
            for (int i = 0; i < gameSaves.Count; i++)
            {
                var currentSave = gameSaves[i];
                if (currentSave.SlotNumber == slotNumber)
                    return currentSave;
            }

            return null;
        }


        protected virtual void UnlistenForEvents()
        {
            saveWriter.GameSaveWritten -= OnGameSaveWritten;
            saveReader.GameSaveRead -= OnGameSaveRead;
            Signals.GameSaveErased -= OnGameSaveErased;
        }

        
    }
}