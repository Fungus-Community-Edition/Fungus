using System.Collections.Generic;

namespace Amanita.SaveSys
{
    public interface ISaveCodec
    {
        /// <summary>
        /// Lower num, earlier execution by the system
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Some encoders may not need any input, so this is optional.
        /// </summary>
        System.Object ToMakeFrom { get; set; }
        bool NeedsInput { get; }
        SaveDataUnit EncodeToUnit();

    }

    /// <summary>
    /// These are the ones you're supposed to pass to the Main Encoders list
    /// in the SaveSystem singleton prefab
    /// </summary>
    public interface IMainSaveCodec : ISaveCodec
    {
        IList<SaveDataUnit> FindAndEncodeAll();
    }

    public interface ISaveCodec<TInput>: ISaveCodec
        where TInput : class
    {
        new TInput ToMakeFrom { get; set; }
    }

    /// <summary>
    /// Creates SaveData out of an object passed to it.
    /// </summary>
    public interface ISaveEncoder<TInput, TOutput> : ISaveCodec<TInput>
        where TInput : class
        where TOutput : SaveData
    {
        TOutput EncodeToSave(TInput from);
    }

    public interface IMultiSaveCodec<TOutput> : ISaveCodec<TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeToMultiSave();
    }

    public interface IMultiSaveCodec<TInput, TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeToMultiSave(TInput toMakeFrom);
    }

    public interface ISaveCodecHandleCheck
    {
        bool CanHandle(System.Object toMakeFrom);
        bool CanHandle(string typeName);
    }


}