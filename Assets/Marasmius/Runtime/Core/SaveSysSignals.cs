using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AtMycelia.SaveSys
{
    public static class SaveSysSignals
    {
        public static Action<SaveWriteResults> AmanitaSaveWritten = delegate { };

        public static Action<SaveDataSet> SaveAddedToSlot = delegate { };
        public static Action<SaveDataSet> SaveRemovedFromSlot = delegate { };
        public static Action<SaveDataSet> SaveInSlotOverwritten = delegate { };
        public static Action<SaveDataSet> SaveInSlotLoaded = delegate { };

        public static Action<int> SaveSlotSelected = delegate { };

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
        /// Executes right after a scene is loaded by the save sys, but before any save data is applied 
        /// to objects in the scene.
        /// </summary>
        public static Func<Task> AfterSceneLoadAsync { get; set; } = delegate { return Task.CompletedTask; };

    }
}