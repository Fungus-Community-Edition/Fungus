using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface ISaveEncoder
    {
        int Priority { get; }
    }

    public interface ISaveEncoder<TSaveDataOutput>: ISaveEncoder
        where TSaveDataOutput : SaveData
    {
        TSaveDataOutput Encode();
    }

    /// <summary>
    /// Creates SaveData out of an object passed to it.
    /// </summary>
    public interface ISaveEncoder<TSaveDataOutput, TMakeFrom> : ISaveEncoder
        where TSaveDataOutput : SaveData
    {
        TSaveDataOutput Encode(TMakeFrom from);
    }

    public interface IMultiSaveEncoder<TSaveDataOutput> : ISaveEncoder<TSaveDataOutput>
        where TSaveDataOutput : SaveData
    {
        IList<TSaveDataOutput> EncodeMulti();
    }

    
}