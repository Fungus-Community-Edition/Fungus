using System;
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

        // To help client code see if this codec can handle the type of object the client
        // wants to pass to it.
        bool CanHandle(System.Object toMakeFrom);
        bool CanHandle(string typeName);

    }

    /// <summary>
    /// These are the ones you're supposed to pass to the Main Encoders list
    /// in the SaveSystem singleton prefab
    /// </summary>
    public interface IMainSaveCodec : ISaveCodec
    {
        /// <summary>
        /// For any setup this codec needs to do during SaveSystem initialization.
        /// </summary>
        void PreInstallInit();

        /// <summary>
        /// The onComplete should contain the produced SaveData instances.
        /// </summary>
        IList<SaveData> FindAndCreateAll(Action<IList<SaveData>> onComplete = null);
    }

    public interface ISaveCodec<TInput>: ISaveCodec
        where TInput : class
    {
        new TInput ToMakeFrom { get; set; }
    }

    /// <summary>
    /// Creates SaveData out of an object passed to it.
    /// </summary>
    public interface ISaveCodec<TNonEncoded, TSaveData> : ISaveCodec<TNonEncoded>
        where TNonEncoded : class
        where TSaveData : SaveData
    {
        TSaveData EncodeToSave(TNonEncoded from);

        TSaveData Decode(string rawText);
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