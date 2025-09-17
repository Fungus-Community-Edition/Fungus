using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using System;
using UnityEngine.UIElements;
using BaseObj = System.Object;

namespace Amanita.EditorUtils
{
    public static class AmanitaEditorSignals
    {
        public static Action<FocusOutEvent> VarRowControlLostFocus = delegate { };
        public static Action<IVariableSource> VariableAdded = delegate { };
        public static Action<IVariableSource> VariableRemoved = delegate { };
        public static Action<VariableRow> VarRowRemoveButtonClicked = delegate { };

        public static Action<BaseObj> ControlValueChanged = delegate { };
    }
}