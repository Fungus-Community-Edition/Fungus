using System;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    public static class AmanitaEditorSignals
    {
        public static Action<FocusOutEvent> VarRowControlLostFocus = delegate { };
    }
}