namespace Amanita.SaveSys
{
    public class SaveRegistrationRequest
    {
        public virtual SaveMetaData SaveMetaData { get; set; }
        public virtual SaveData MainSaveData { get; set; }
        public virtual int SlotNumber { get; set; }
    }
}