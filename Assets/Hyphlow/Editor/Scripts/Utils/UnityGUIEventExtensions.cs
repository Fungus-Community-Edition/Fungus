using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class UnityGUIEventExtensions 
    {
        /// <summary>
        /// Returns true if the event is a left mouse button click. Note that this will be
        /// true for both clicks of a double-click.
        /// </summary>
        public static bool LeftClick(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && guiEvent.clickCount > 0;
        }

        /// <summary>
        /// Returns true if the event is a left mouse button click or down.
        /// </summary>
        public static bool LeftMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 0;
        }

        /// <summary>
        /// Returns true if the event is a right mouse button click or down.
        /// </summary>
        public static bool RightClick(this Event guiEvent)
        {
            return guiEvent.RightMouseButton() && guiEvent.clickCount > 0;
        }

        /// <summary>
        /// Returns true if the event is a right mouse button click. Note that 
        /// this will be true for both clicks of a double-click.
        /// </summary>
        public static bool RightMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 1;
        }

        /// <summary>
        /// Returns true if the event is a middle mouse button click. Note that this 
        /// will be true for both clicks of a double-click.
        /// </summary>
        public static bool MiddleMouseButton(this Event guiEvent)
        {
            return guiEvent.button == 2;
        }

        /// <summary>
        /// Returns true if the event is a left mouse button double-click. 
        /// Note that this will be true for the second click of a double-click, 
        /// but not the initial click.
        public static bool DoubleClick(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && guiEvent.clickCount > 1;
        }

        /// <summary>
        /// Returns true if the event is a middle mouse drag, or an alt-left-drag (which is also used for panning).
        /// </summary>
        public static bool PanInput(this Event guiEvent)
        {
            bool altLeftDrag = guiEvent.LeftMouseButton() && guiEvent.alt;
            return guiEvent.MiddleMouseButton() || altLeftDrag;
        }

        /// <summary>
        /// Returns true if the event is a left mouse drag that is not also an 
        /// alt-left-drag (which is used for panning).
        public static bool LeftDragInput(this Event guiEvent)
        {
            return guiEvent.LeftMouseButton() && !guiEvent.alt && guiEvent.delta != Vector2.zero;
        }

        /// <summary>
        /// Returns true if the event is a right mouse drag. 
        public static bool RightDragInput(this Event guiEvent)
        {
            return guiEvent.RightMouseButton() && guiEvent.delta != Vector2.zero;
        }

        /// <summary>
        /// Returns true if the event is a mouse down event of any button. Note that this will be 
        /// true for the initial click of a double-click, but not the second click.
        public static bool MouseDown(this Event guiEvent)
        {
            return guiEvent.type == EventType.MouseDown;
        }
    }
}