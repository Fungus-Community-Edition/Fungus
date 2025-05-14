using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

namespace CGTUnity.Fungus.SaveSystem
{
    /// <summary>
    /// The main interface for creating, loading, and deleting saves.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager S { get; private set; }
        
        #region Fields
        // Most of the functionality here is passed off to the following submodules. Much of what 
        // this does without passing the job is react to that the submodules do, hence the 
        // stuff in the event-listener region.
        [SerializeField] protected SaveWriter saveWriter;
        [SerializeField] protected SaveReader saveReader;
        [SerializeField] protected GameLoader gameLoader;
        [SerializeField] protected GameSaver gameSaver;
        [Tooltip("Whether or not this should display warnings when asked to add saves when it isn't allowed to.")]
        [SerializeField] protected bool warnOnUnallowedSave = true;
        

        protected static List<GameSaveData> gameSaves = new List<GameSaveData>();
        protected static List<GameSaveData> unwrittenSaves = new List<GameSaveData>();
        public static Dictionary<string, GameSaveData> WrittenSaves { get; private set; }
        // ^ Keeping track of what's written or unwritten helps optimize the save-writing 
        // and save-deleting processes.

        static SaveManager()
        {
            WrittenSaves = new Dictionary<string, GameSaveData>();
            WrittenSaves = new Dictionary<string, GameSaveData>();
        }

        [Tooltip("Whether or not this SaveManager can add saves.")]
        [SerializeField] bool savingEnabled = true;
        public virtual bool SavingEnabled
        {
            get { return savingEnabled; }
            set { savingEnabled = value; }
        }

        [Tooltip("How long to keep self from saving when this SaveManager first starts up.")]
        [SerializeField] protected float awakeSaveDelay = 2f;

        public virtual float AwakeSaveDelay
        {
            get { return awakeSaveDelay; }
            set { awakeSaveDelay = value; }
        }

        #endregion

        #region Properties

        public virtual SaveReader SaveReader
        {
            get { return saveReader; }
        }
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
#endregion

#region Methods
        
#region MonoBehaviour Standard
        protected virtual void Awake()
        {
            if (S != null && S != this)
            {
                Destroy(this.gameObject);
                return;
            }

            S = this;

            // Get the necessary components
            if (gameLoader == null) gameLoader = FindObjectOfType<GameLoader>();
            if (gameSaver == null) gameSaver = FindObjectOfType<GameSaver>();

            if (!Directory.Exists(SaveDirectory))
                Directory.CreateDirectory(SaveDirectory);

            ListenForEvents();
            if (awakeSaveDelay > 0)
                StartCoroutine(ExecuteSaveDelay());
        }

        IEnumerator ExecuteSaveDelay()
        {
            bool previous = savingEnabled;
            savingEnabled = false;
            yield return new WaitForSeconds(awakeSaveDelay);
            savingEnabled = previous;
        }

        protected virtual void Start()
        {
            // So other objects (like the SaveSlotManager) can be ready to listen
            // for the save-reading
            if (gameSaves.Count == 0)
                saveReader.ReadAllFromDisk(SaveDirectory);

        }

        protected virtual void OnDestroy()
        {
            UnlistenForEvents();
        }

#endregion

#region Event Listeners
        protected virtual void OnGameSaveWritten(GameSaveData saveData, string filePath, string fileName)
        {
            unwrittenSaves.Remove(saveData);
            WrittenSaves[fileName] = saveData;
        }

        protected virtual void OnGameSaveRead(GameSaveData saveData, string filePath, string fileName)
        {
            WrittenSaves[fileName] = saveData;
            if (!gameSaves.Contains(saveData))
                gameSaves.Add(saveData);
        }

        protected virtual void OnGameSaveErased(GameSaveData saveData, string filePath, string fileName)
        {
            gameSaves.Remove(saveData);
            unwrittenSaves.Remove(saveData);
            WrittenSaves[fileName] = null;
        }

#endregion

#region Save-writing

        /// <summary>
        /// Writes all the unwritten saves this is keeping track of to disk.
        /// </summary>
        public virtual void WriteSavesToDisk()
        {
            saveWriter.WriteAllToDisk(unwrittenSaves, SaveDirectory);
        }

        /// <summary>
        /// Writes the passed save data to disk.
        /// </summary>
        public virtual void WriteSaveToDisk(GameSaveData saveData)
        {
            saveWriter.WriteOneToDisk(saveData, SaveDirectory);
        }

#endregion

#region Save-creation and registration

        /// <summary>
        /// Creates and registers new save data with the passed slot's number, then writing it to disk
        /// if set to do so. Save replacement may happen depending on the aforementioned number.
        /// </summary>
        public virtual bool AddSave(SaveSlot slot, bool writeToDisk = true)
        {
            if (!savingEnabled)
            {
                if (warnOnUnallowedSave)
                    Debug.LogWarning(this.name + "'s SaveManager is set to not add saves.");
                return false;
            }

            if (slot == null)
                throw new System.NullReferenceException("Cannot register a save with a null slot's number.");

            return AddSave(slot.Number, writeToDisk);
        }

        /// <summary>
        /// Creates and registers new save data with the passed slot number, then writing it to disk
        /// if set to do so. Save replacement may happen depending on the aforementioned number.
        /// </summary>
        public virtual bool AddSave(int slotNumber, bool writeToDisk = true)
        {
            if (!savingEnabled)
            {
                if (warnOnUnallowedSave)
                    Debug.LogWarning(this.name + "'s SaveManager is set to not add saves.");
                return false;
            }

            var newSaveData = gameSaver.CreateSave(slotNumber);
            return AddSave(newSaveData, writeToDisk);
        }

        /// <summary>
        /// Adds a save to the manager. If the passed save shares a number with one it's already
        /// keeping track of, the old one is replaced with the new one.
        /// In which case, the new one will be written to disk regardless of the 
        /// second argument.
        /// </summary>
        public virtual bool AddSave(GameSaveData newSave, bool writeToDisk = true)
        {
            if (newSave == null)
                return false;

            if (!savingEnabled)
            {
                if (warnOnUnallowedSave)
                    Debug.LogWarning(this.name + "'s SaveManager is set to not add saves.");
                return false;
            }

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
                if (!gameSaves.Contains(newSave))
                    gameSaves.Add(newSave);
                unwrittenSaves.Add(newSave);

                // Write if appropriate.
                if (writeToDisk)
                    WriteSaveToDisk(newSave);
            }

            return true;
        }

#endregion

#region Save-erasing
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

            foreach (var fileName in WrittenSaves.Keys)
            {
                var metaFileName = fileName + ".meta";

                if (WrittenSaves[fileName] == saveData)
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
#endregion

#region Save-loading
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

#endregion

#region Save-retrieval
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
#endregion

#region Helpers
        // When it comes to saves being read or written, this manager only cares when it's the specified
        // save readers and writers doing it.
        protected virtual void ListenForEvents()
        {
            saveWriter.GameSaveWritten += OnGameSaveWritten;
            saveReader.GameSaveRead += OnGameSaveRead;
            Signals.GameSaveErased += OnGameSaveErased;
        }

        protected virtual void UnlistenForEvents()
        {
            saveWriter.GameSaveWritten -= OnGameSaveWritten;
            saveReader.GameSaveRead -= OnGameSaveRead;
            Signals.GameSaveErased -= OnGameSaveErased;
        }

        /// <summary>
        /// Both saves are assumed to have the same slot number.
        /// </summary>
        protected virtual void ReplaceSave(GameSaveData oldSave, GameSaveData newSave)
        {
            gameSaves.Remove(oldSave);
            gameSaves.Add(newSave);

            var writeNewSave = false;

            // If the dict had the old save data, write the new one to replace it. Using foreach due to
            // dict limitations.
            foreach (var fileName in WrittenSaves.Keys)
            {
                if (WrittenSaves[fileName] == oldSave)
                {
                    writeNewSave = true;
                    break;
                }
            }

            if (writeNewSave)
            {
                WriteSaveToDisk(newSave);
            }
            else
            {
                unwrittenSaves.Remove(oldSave);
                unwrittenSaves.Add(newSave);
            }
        }
#endregion
#endregion

    }
}