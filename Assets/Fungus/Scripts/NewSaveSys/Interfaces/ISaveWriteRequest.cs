namespace Amanita.SaveSys
{
    public interface ISaveWriteRequest
    {
        int SlotNumber { get; set; }
        public ISaveData MainState { get; set; }
        public ISaveMetaData SaveMetaData { get; set; }
        public SaveDirectoryType BaseSaveDirectory { get; set; }
    }
}