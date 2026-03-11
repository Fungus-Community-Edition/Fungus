using System;

namespace AtMycelia.UI
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