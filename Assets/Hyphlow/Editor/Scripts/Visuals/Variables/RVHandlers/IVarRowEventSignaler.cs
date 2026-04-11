using System;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IVarRowEventSignaler
    {
        event Action<IRowVisualHandler> RemoveButtonClicked;
        event Action<TextField> KeyFieldChanged;
        event Action<VariableScope> ScopeFieldChanged;
        event Action<object> ValueFieldChanged;
    }
}