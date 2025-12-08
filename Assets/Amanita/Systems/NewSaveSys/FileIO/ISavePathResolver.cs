namespace Amanita.SaveSys
{
    // Credit to Bayat Games for inspiring me to come up with this idea
    public interface ISavePathResolver : ISaveFilePathResolver, ISaveFolderPathResolver
    {
        
    }

    public interface ISaveFolderPathResolver
    {
        string RelativePath { get; }
        string GetSaveFolderPath(object input);
    }


    public interface ISaveFolderPathResolver<TInput> : ISaveFolderPathResolver
    {
        string GetSaveFolderPath(TInput input);
    }

    public interface ISaveFilePathResolver
    {
        string FileExtension { get; }
        string GetSaveFilePath(string fileName, object input);
    }

    public interface ISaveFilePathResolver<TInput> : ISaveFilePathResolver
    {
        string GetSaveFilePath(string fileName, TInput input);
    }

    public interface ISavePathResolver<TInput> : ISavePathResolver
    {
        string GetSaveFolderPath(TInput input);
        string GetSaveFilePath(string fileName, TInput input);
    }

    public interface ISaveSlotPathResolver : ISavePathResolver
    {
        string NumberFormat { get; }
        string GetSaveFileName(int slotNumber);
        string GetSaveFilePath(object input, int slotNumber);
    }

    public interface ISaveSlotPathResolver<TInput> : ISavePathResolver<TInput>, ISaveSlotPathResolver
    {
        string GetSaveFilePath(TInput input, int slotNumber);
    }

    public interface IConfigurableSavePathResolver : ISavePathResolver
    {
        new string RelativePath { get; set; }
        new string FileExtension { get; set; }
    }

    public interface IConfigurableSavePathResolver<TInput> : IConfigurableSavePathResolver, ISavePathResolver<TInput>
    {
    }

    public interface IConfigurableSaveSlotPathResolver : IConfigurableSavePathResolver, ISaveSlotPathResolver
    {
        new string NumberFormat { get; set; }
    }

    public interface IHasConfigurableSaveSlotPathResolver
    {
        IConfigurableSaveSlotPathResolver PathResolver { get; set; }
    }

    public interface IConfigurableSaveSlotPathResolver<TInput> : IConfigurableSavePathResolver<TInput>, IConfigurableSaveSlotPathResolver, ISaveSlotPathResolver<TInput>
    {
    }
}
