using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface ISaveEncoder
    {
        int Priority { get; }

        /// <summary>
        /// Some encoders may not need any input, so this is optional.
        /// </summary>
        System.Object ToMakeFrom { get; set; }
        bool NeedsInput { get; }
        SaveDataUnit EncodeToUnit();
        
    }

    public interface ISaveEncoder<TInput>: ISaveEncoder
        where TInput : class
    {
        new TInput ToMakeFrom { get; set; }
    }

    /// <summary>
    /// Creates SaveData out of an object passed to it.
    /// </summary>
    public interface ISaveEncoder<TInput, TOutput> : ISaveEncoder<TInput>
        where TInput : class
        where TOutput : SaveData
    {
        TOutput EncodeToSave(TInput from);
    }

    public interface IMultiSaveEncoder<TOutput> : ISaveEncoder<TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeToMultiSave();
    }

    public interface IMultiSaveEncoder<TInput, TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeToMultiSave(TInput toMakeFrom);
    }

    public interface ISaveEncoderHandleCheck
    {
        bool CanHandle(System.Object toMakeFrom);
        bool CanHandle(string typeName);
    }


}