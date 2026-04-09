using System;
using UnityObj = UnityEngine.Object;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom("AtMycelia.Amanita.VScripting")]
    public static class VariableSignals 
    {
        public static Action<IVariable> PreValueChange = delegate { };

        /// <summary>
        /// The object param is the old value of the variable. Getting the new one
        /// is obvious.
        /// </summary>
        public static Action<IVariable, object> PostValueChange = delegate { };

    }
}