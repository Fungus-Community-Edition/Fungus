namespace Amanita.SaveSys
{
    /// <summary>
    /// This should always be assigned a meta, but not always a main. Better to
    /// read mains from disk on demand rather than right on system startup.
    /// </summary>
    public class SaveDataSet : ISaveDataSet
    {
        public SaveDataSet(ISaveMetaData meta, ISaveData mainData = null)
        {
            Meta = meta;
            MainState = mainData;
        }

        public virtual ISaveMetaData Meta
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
        protected ISaveMetaData _meta;

        public virtual ISaveData MainState { get; set; }
        public virtual int SlotNumber { get { return Meta.SlotNumber; } }

    }

}