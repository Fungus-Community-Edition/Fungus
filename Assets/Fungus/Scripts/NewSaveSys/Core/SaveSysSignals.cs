using UnityEngine;
using UnityEngine.Events;

namespace Amanita.SaveSys
{
    public static class SaveSysSignals
    {
        public static UnityAction<SaveWriteResults> AmanitaSaveWritten = delegate { };

        public static UnityAction<SaveDataSet> SaveAddedToSlot = delegate { };
        public static UnityAction<SaveDataSet> SaveRemovedFromSlot = delegate { };
        public static UnityAction<SaveDataSet> SaveInSlotOverwritten = delegate { };
    }
}