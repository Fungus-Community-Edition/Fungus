using System;

namespace CGT.UI
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