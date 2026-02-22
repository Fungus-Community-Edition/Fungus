using System;

namespace Amanita.VScripting
{
    public static class VsaSignals
    {
        public static Action<VariableSourceAsset> VsaEnabled = delegate { };
        public static Action<VariableSourceAsset> VsaDisabled = delegate { };
    }
}