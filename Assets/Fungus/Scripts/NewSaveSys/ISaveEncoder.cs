using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface ISaveEncoder
    {
        int Priority { get; }
        SaveDataUnit Encode(System.Object toMakeFrom);
        bool CanHandle(System.Object toMakeFrom);
        bool CanHandle(string typeName);
    }

    public interface ISaveEncoder<TInput>: ISaveEncoder
        where TInput : class
    {
        SaveDataUnit Encode(TInput toMakeFrom = null);
        bool CanHandle(TInput toMakeFrom);
    }

    /// <summary>
    /// Creates SaveData out of an object passed to it.
    /// </summary>
    public interface ISaveEncoder<TInput, TOutput> : ISaveEncoder
    {
        TOutput Encode(TInput from);
    }

    public interface IMultiSaveEncoder<TOutput> : ISaveEncoder<TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeMulti();
    }

    public interface IMultiSaveEncoder<TInput, TOutput>
        where TOutput : SaveData
    {
        IList<TOutput> EncodeMulti(TInput toMakeFrom);
    }


}