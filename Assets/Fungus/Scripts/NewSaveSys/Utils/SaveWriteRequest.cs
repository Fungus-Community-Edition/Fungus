using System;
using UnityEngine;

namespace Amanita.SaveSys
{

    [System.Serializable]
    public class SaveWriteRequest : EventArgs
    {
        public string SaveName { get; set; } = string.Empty;
        public virtual int SlotNumber { get; set; } = 0;
        public SaveData SaveData { get; set; }
        public SaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveWriteRequest() { }

    }
}