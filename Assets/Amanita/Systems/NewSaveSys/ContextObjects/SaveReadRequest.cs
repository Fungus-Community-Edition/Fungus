using System;

namespace Amanita.SaveSys
{
    public class SaveReadRequest : EventArgs
    {
        public virtual int SlotNumber { get; set; } = 0;
        public virtual SaveDirectoryType BaseSaveDirectory { get; set; } = SaveDirectoryType.DataPath;
        public SaveReadRequest()
        {

        }

        public SaveReadRequest(SaveReadRequest other)
        {
            this.SlotNumber = other.SlotNumber;
            BaseSaveDirectory = other.BaseSaveDirectory;
        }

        public virtual void Clear()
        {
            SlotNumber = 0;
            BaseSaveDirectory = SaveDirectoryType.DataPath;
        }
    }
}