using Amanita.SaveSys;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface IDecryptor
    {
        ISaveMetaData DecryptMeta(object input);
        ISaveData DecryptMainState(object input);
        ISaveDataSet DecryptWholeSet(object input);
    }
}