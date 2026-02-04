using UnityEngine;
using UnityEngine.UIElements;
namespace Amanita.EditorUtils
{
    public static class PointerEventExtensions
    {
        public static bool IsLeftMouseButtonPressed(this IPointerEvent evt)
        {
            return (evt.pressedButtons & 1) != 0;
        }

        public static bool IsRightMouseButtonPressed(this IPointerEvent evt)
        {
            return (evt.pressedButtons & 2) != 0;
        }

        public static bool IsPanInput(this IPointerEvent evt)
        {
            return evt.IsMiddleMouseButtonPressed() || evt.IsAltLeftDrag();
        }

        public static bool IsMiddleMouseButtonPressed(this IPointerEvent evt)
        {
            return (evt.pressedButtons & 4) != 0;
        }

        public static bool IsAltLeftDrag(this IPointerEvent evt)
        {
            return (evt.pressedButtons & 1) != 0 && evt.altKey;
        }
    }
}