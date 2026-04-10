using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom("AtMycelia.Hyphlow")]
    public static class VsaSignals
    {
        public static Action<VariableSourceAsset> VsaEnabled = delegate { };
        public static Action<VariableSourceAsset> VsaDisabled = delegate { };
    }
}