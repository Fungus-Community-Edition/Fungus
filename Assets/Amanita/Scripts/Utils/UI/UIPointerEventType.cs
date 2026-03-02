using System;

namespace AtMycelia.Amanita.UI
{
    [Flags]
    public enum UIPointerEventType
    {
        Null,
        Click,
        Up,
        Down,
        Enter,
        Exit,
        BeginDrag,
        Drag,
        EndDrag,
        Drop
    }
}