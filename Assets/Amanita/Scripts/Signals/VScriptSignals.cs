using System;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Signals for the more general visual-scripting aspects of Amanita.
    /// </summary>
    public static class VScriptSignals
    {
        public static Action<IHasUniqueID> UniqueGuidAssigned = delegate { };
        public static Action<IHasUniqueID> UniqueIDHaverEnabled = delegate { };
    }
}