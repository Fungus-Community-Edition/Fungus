namespace AtMycelia.SaveSys
{
    public class SaveWriteResults : System.EventArgs
    {
        public virtual string FilePath { get; set; }
        public virtual string FileName { get; set; }
        public virtual CompositeSaveData MainSaveData { get; set; }
        public virtual ISaveMetaData Meta { get; set; }
        public virtual bool Success { get; set; }
        public virtual string ErrorMessage { get; set; }
        public virtual SaveWriteRequest Request { get; set; }
        public virtual int SlotNumber => Request.SlotNumber;
    }
}