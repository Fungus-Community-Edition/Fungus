using System;
using UnityEngine.UIElements;
using BaseObj = System.Object;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class HyphlowEditorSignals
    {
        public static Action<FocusOutEvent> VarRowControlLostFocus = delegate { };
        public static Action<IVariable> VariableAdded = delegate { };
        public static Action<IVariable> VariableRemoved = delegate { };
        public static Action<VariableRow> VarRowRemoveButtonClicked = delegate { };

        public static Action<BaseObj> ControlValueChanged = delegate { };
        public static Action<VariableRow, string> KeyFieldChanged = delegate { };
        public static Action<VariableRow, VariableScope> ScopeFieldChanged = delegate { };
        public static Action<VariableRow, object> ValueFieldChanged = delegate { };

    }
}