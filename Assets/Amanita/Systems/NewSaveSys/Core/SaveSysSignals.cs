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
    }
}