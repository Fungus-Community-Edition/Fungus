namespace AtMycelia.SaveSys
{
    public interface ISaveDataSet
    {
        ISaveMetaData Meta { get; }
        ISaveData MainState { get; }
    }
}