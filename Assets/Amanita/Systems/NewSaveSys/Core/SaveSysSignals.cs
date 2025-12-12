using System;
using System.Collections.Generic;

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
        public static Action<IList<ISaveMetaData>> SaveMetasReadOnInit = delegate { };

        public static Action BaseSaveSysInstallationComplete = delegate { };

        public static Action SaveMenuOpened = delegate { };
        public static Action SaveMenuClosed = delegate { };
    }
}