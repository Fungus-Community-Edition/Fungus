using System;
using UnityEngine;

namespace Amanita.SaveSys
{

    [System.Serializable]
    public class SaveWriteRequest : EventArgs, ISaveWriteRequest
    {
        public string SaveName { get; set; } = string.Empty;
        public virtual int SlotNumber { get; set; } = 0;
        public ISaveData MainSaveData { get; set; }
        public ISaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveWriteRequest() { }

    }

    public interface ISaveWriteRequest
    {
        int SlotNumber { get; set; }
        public ISaveData MainSaveData { get; set; }
        public ISaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; }
    }
}