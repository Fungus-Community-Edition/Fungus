using System;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Detects raw IMGUI input inside the UITK flowchart window and relays it to FlowchartWindowSignals.
    /// Call <see cref="OnGUI(Event)"/> from the owning window’s OnGUI loop.
    /// </summary>
    public sealed class InputSignalModuleUitk : IFlowchartWindowModule, IDisposable
    {
        private bool isDisposed;
        private Vector2? activePanAnchor;
        // When the user is panning with middle mouse button (or alt + left), we need to keep 
        // track of the last mouse position to calculate deltas. That last mouse position
        // is stored here.

        public void Initialize(FlowchartWindowUitk window)
        {
            
        }

        public void OnGUI(Event guiEvent)
        {
            if (isDisposed || guiEvent == null)
            {
                return;
            }

            switch (guiEvent.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown(guiEvent);
                    break;

                case EventType.MouseUp:
                    HandlePanInputRelease(guiEvent);
                    break;

                case EventType.MouseDrag:
                    HandleMouseDrag(guiEvent);
                    break;

                case EventType.ScrollWheel:
                    HandleScrollWheel(guiEvent);
                    break;
            }
        }

        private void HandleMouseDown(Event guiEvent)
        {
            if (IsLeftClick(guiEvent))
            {
                if (IsDoubleClick(guiEvent))
                {
                    Debug.Log("Double click detected");
                    FlowchartWindowSignals.DoubleClicked(guiEvent.mousePosition);
                }
                else
                {
                    Debug.Log("Left click detected");
                    FlowchartWindowSignals.LeftClicked(guiEvent.mousePosition);
                }
            }
            else if (IsRightClick(guiEvent))
            {
                Debug.Log("Right click detected");
                FlowchartWindowSignals.RightClicked(guiEvent.mousePosition);
            }

            if (IsPanInput(guiEvent))
            {
                Debug.Log("Pan input started");
                activePanAnchor = guiEvent.mousePosition;
            }
        }

        private static bool IsLeftClick(Event guiEvent) => guiEvent.button == 0 && guiEvent.clickCount > 0;
        private static bool IsDoubleClick(Event guiEvent) => guiEvent.button == 0 && guiEvent.clickCount > 1;
        private static bool IsRightClick(Event guiEvent) => guiEvent.button == 1 && guiEvent.clickCount == 1;

        private static bool IsPanInput(Event guiEvent)
        {
            bool middleButtonDown = guiEvent.button == 2;
            bool altLeftDrag = guiEvent.button == 0 && guiEvent.alt;
            return middleButtonDown || altLeftDrag;
        }

        private void HandlePanInputRelease(Event guiEvent)
        {
            if (guiEvent.button == 2 || (guiEvent.button == 0 && !guiEvent.alt))
            {
                activePanAnchor = null;
            }
        }

        private void HandleMouseDrag(Event guiEvent)
        {
            if (!activePanAnchor.HasValue || !IsPanInput(guiEvent))
            {
                Debug.Log("No active pan anchor or not pan input");
                return;
            }

            Vector2 movementSinceLastFrame = guiEvent.mousePosition - activePanAnchor.Value;
            if (movementSinceLastFrame.sqrMagnitude > Mathf.Epsilon)
            {
                Debug.Log("Panning");
                FlowchartWindowSignals.ScrollWheelDragged(movementSinceLastFrame);
                FlowchartWindowSignals.WindowPanned();
            }

            activePanAnchor = guiEvent.mousePosition;
            guiEvent.Use();
        }

        private static void HandleScrollWheel(Event guiEvent)
        {
            FlowchartWindowSignals.ScrollWheelMoved();

            if (guiEvent.delta.sqrMagnitude > Mathf.Epsilon)
            {
                FlowchartWindowSignals.ScrollWheelDragged(guiEvent.delta);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            activePanAnchor = null;
        }
    }
}