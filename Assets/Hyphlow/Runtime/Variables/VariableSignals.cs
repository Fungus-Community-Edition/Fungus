using System;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VariableSignals 
    {
        public static Action<IVariable> PreValueChange = delegate { };

        /// <summary>
        /// The object param is the old value of the variable. Getting the new one
        /// is obvious.
        /// </summary>
        public static Action<IVariable, object> PostValueChange = delegate { };

        public static Action<IVariable> VariableAdded = delegate { };
        public static Action<IVariable> VariableRemoved = delegate { };

#if UNITY_EDITOR
        public static Action<IVariable> EditorValueChange = delegate { };
#endif

    }
}