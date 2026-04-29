using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VsaSignals
    {
        /// <summary>
        /// Should execute right before a variable is to be added to a VSA.
        /// </summary>
        public static Action<VariableSourceAsset, IVariable> PreVariableAdded = delegate { };

        /// <summary>
        /// Should execute right before a variable is to be removed from a VSA.
        /// </summary>
        public static Action<VariableSourceAsset, IVariable> PreVariableRemoved = delegate { };


        public static Action<VariableSourceAsset, IVariable> VariableAdded = delegate { };
        public static Action<VariableSourceAsset, IVariable> VariableRemoved = delegate { };

        public static Action<VariableSourceAsset> VsaEnabled = delegate { };
        public static Action<VariableSourceAsset> VsaDisabled = delegate { };
    }
}