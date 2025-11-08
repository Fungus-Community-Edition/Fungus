using System;

namespace Amanita.SaveSys
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
        public static Action SaveReadInitDone = delegate { };
    }
}