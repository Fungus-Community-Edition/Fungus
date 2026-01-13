using System;

namespace Amanita.DialogueSys
{
    public static class DialogueSysSignals
    {
        public static Action<SayDialog> SayDialogEnabled = delegate { };
        public static Action<SayDialog> SayDialogDisabled = delegate { };

        /// <summary>
        /// "Made" as in instantiated.
        /// </summary>
        public static Action<SayDialog, SayDialog> SayDialogMadeFromPrefab = delegate { };
    }
}