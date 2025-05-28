namespace Amanita.SaveSys
{
    /// <summary>
    /// This should always be assigned a meta, but not always a main. Better to
    /// read mains from disk on demand rather than right on system startup.
    /// </summary>
    public class SaveDataSet
    {
        public SaveDataSet(SaveMetaData meta, SaveData mainData = null)
        {
            Meta = meta;
            MainData = mainData;
        }

        public virtual SaveMetaData Meta
        {
            get
            {
                return _meta;
            }

            set
            {
                if (value == null)
                {
                    string errorMessage = $"Tried to assign a null meta to a SaveDataSet. That ain't allowed, cowboi!";
                    throw new System.ArgumentNullException(nameof(Meta), errorMessage);
                }

                _meta = value;

            }
        }
        protected SaveMetaData _meta;

        public virtual SaveData MainData { get; set; }
        public virtual int SlotNumber { get { return Meta.SlotNumber; } }

    }
}