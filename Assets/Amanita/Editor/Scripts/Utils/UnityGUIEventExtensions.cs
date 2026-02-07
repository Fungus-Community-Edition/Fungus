using UnityEngine;

namespace Amanita.EditorUtils
{
    public static class UnityGUIEventExtensions 
    {
        public static bool LeftClick(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && guiEvent.clickCount > 0;
        }

        public static bool LeftMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 0;
        }

        public static bool RightClick(this Event guiEvent)
        {
            return guiEvent.RightMouseButton() && guiEvent.clickCount > 0;
        }

        public static bool RightMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 1;
        }

        public static bool MiddleMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 2;
        }

        public static bool DoubleClick(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && guiEvent.clickCount > 1;
        }

        public static bool PanInput(this Event guiEvent)
        {
            bool altLeftDrag = guiEvent.LeftMouseButton() && guiEvent.alt;
            return guiEvent.MiddleMouseButton() || altLeftDrag;
        }

        public static bool LeftDragInput(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && !guiEvent.alt;
        }

        public static bool RightDragInput(this Event guiEvent)
        {
            return guiEvent.RightMouseButton();
        }
    }
}