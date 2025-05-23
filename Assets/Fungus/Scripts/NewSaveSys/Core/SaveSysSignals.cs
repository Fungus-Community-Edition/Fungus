using UnityEngine;
using UnityEngine.Events;

namespace Amanita.SaveSys
{
    public static class SaveSysSignals
    {
        public static UnityAction<AmanitaSaveData, string, string> AmanitaSaveWritten = delegate { };
    }
}