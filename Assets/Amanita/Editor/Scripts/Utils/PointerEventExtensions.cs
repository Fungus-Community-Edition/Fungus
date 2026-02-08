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

        public static void ApplyButtonTo(this IPointerEvent evt, Event systemEvent)
        {
            if (evt.IsLeftMouseButtonPressed())
            {
                systemEvent.button = 0;
            }
            else if (evt.IsRightMouseButtonPressed())
            {
                systemEvent.button = 1;
            }
            else if (evt.IsMiddleMouseButtonPressed())
            {
                systemEvent.button = 2;
            }
        }
    }
}