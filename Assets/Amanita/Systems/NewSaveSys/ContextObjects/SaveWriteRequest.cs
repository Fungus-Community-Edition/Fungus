using System;

namespace Amanita.SaveSys
{

    [System.Serializable]
    /// <summary>
    /// To be passed to SaveWriter to write a save file.
    /// <see cref="SaveWriter"/>"/>
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

        public virtual void Clear()
        {
            SaveName = string.Empty;
            SlotNumber = 0;
            MainState = null;
            SaveMetaData = null;
            BaseSaveDirectory = SaveDirectoryType.DataPath;
        }

    }

}