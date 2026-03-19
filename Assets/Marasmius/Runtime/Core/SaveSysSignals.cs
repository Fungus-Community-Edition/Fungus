using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace AtMycelia.SaveSys
{
    public static class SaveSysSignals
    {
        public static Action<SaveWriteRequest> PreSaveWrittenToEmptySlot = delegate { };
        public static Action<SaveWriteResults> PostSaveWrittenToEmptySlot = delegate { };

        /// <summary>
        /// Happens right before a save starts getting written to a slot that already has 
        /// a save in it at the time, thus overwriting it.
        /// </summary>
        public static Action<SaveWriteRequest> PreSaveOverwritten = delegate { };
        /// <summary>
        /// Executes right after a save is written to a slot that already had a save in it at the 
        /// time, thus overwriting it. 
        /// </summary>
        public static Action<SaveWriteResults> PostSaveOverwritten = delegate { };

        /// <summary>
        /// Executes when a save data set is added to the registry. Note that this is not triggered 
        /// when a save is loaded, only when it's added to the registry. The set may not
        /// necessarily have a main save data instance in it; it should guarantee a meta, however.
        /// </summary>
        public static Action<SaveDataSet> SaveAdded = delegate { };
        public static Action<SaveDataSet> SaveRemoved = delegate { };

        
        /// <summary>
        /// Executes when a save is loaded, with all appliers having finished their jobs.
        /// </summary>
        public static Action<CompositeSaveData> SaveLoaded = delegate { };
        public static Action<Scene> SceneLoaded = delegate { };

        public static Action<int> SlotSelected = delegate { };

        /// <summary>
        /// To be triggered when the system has finished initializing save data reading on startup.
        /// </summary>
        public static Action<IList<ISaveMetaData>> SaveMetasReadOnInit = delegate { };

        public static Action BaseSaveSysInstallationComplete = delegate { };

        public static Action SaveMenuOpened = delegate { };
        public static Action SaveMenuClosed = delegate { };

        /// <summary>
        /// Executes right before a scene is to be loaded by the save sys.
        /// </summary>
        public static Func<Task> BeforeSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

        /// <summary>
        /// Executes right after a scene is loaded by the save sys (with all appliers having finished doing 
        /// their thing).
        /// </summary>
        public static Func<Task> AfterSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

    }
}