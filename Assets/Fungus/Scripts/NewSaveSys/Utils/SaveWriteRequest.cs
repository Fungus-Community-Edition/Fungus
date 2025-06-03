using System;

namespace Amanita.SaveSys
{

    [System.Serializable]
    public class SaveWriteRequest : EventArgs, ISaveWriteRequest
    {
        public string SaveName { get; set; } = string.Empty;
        public virtual int SlotNumber { get; set; } = 0;
        public ISaveData MainState { get; set; }
        public ISaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveWriteRequest() { }

        public SaveWriteRequest(SaveWriteRequest other)
        {
            SaveName = other.SaveName;
            SlotNumber = other.SlotNumber;
            MainState = other.MainState;
            SaveMetaData = other.SaveMetaData;
            BaseSaveDirectory = other.BaseSaveDirectory;
        }

    }

    public interface ISaveWriteRequest
    {
        int SlotNumber { get; set; }
        public ISaveData MainState { get; set; }
        public ISaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; }
    }
}