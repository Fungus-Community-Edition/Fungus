using Amanita.EditorUtils;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Detects raw IMGUI input inside the UITK flowchart window and relays it to FlowchartWindowSignals.
    /// Call <see cref="OnGUI(Event)"/> from the owning window’s OnGUI loop.
    /// </summary>
    public sealed class InputSignalModuleUitk : IFlowchartWindowModule, IDisposable
    {
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
                    HandleLeftMouseDown(guiEvent);
                    break;

                case EventType.MouseUp:
                    HandleLeftMouseUp(guiEvent);
                    HandlePanInputRelease(guiEvent);
                    HandleLeftDragRelease(guiEvent);
                    HandleRightDragRelease(guiEvent);
                    break;

                case EventType.MouseDrag:
                    HandleMouseDrag(guiEvent);
                    break;

                case EventType.ScrollWheel:
                    HandleScrollWheel(guiEvent);
                    break;
            }
        }

        private bool isDisposed;

        private void HandleMouseDown(Event guiEvent)
        {
            if (guiEvent.LeftClick())
            {
                if (guiEvent.DoubleClick())
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
            else if (guiEvent.RightClick())
            {
                Debug.Log("Right click detected");
                FlowchartWindowSignals.RightClicked(guiEvent.mousePosition);
            }

            if (guiEvent.PanInput())
            {
                Debug.Log("Pan input started");
                activePanAnchor = guiEvent.mousePosition;
            }
        }

        private void HandleLeftMouseDown(Event guiEvent)
        {
            if (!guiEvent.LeftMouseButton() || guiEvent.alt)
            {
                return;
            }

            if (!IsMouseOverBlock(guiEvent.mousePosition))
            {
                FlowchartWindowSignals.EmptySpaceLeftMouseDown(guiEvent.mousePosition, guiEvent);
            }
        }

        private void HandleLeftMouseUp(Event guiEvent)
        {
            if (!guiEvent.LeftMouseButton() || guiEvent.alt)
            {
                return;
            }

            FlowchartWindowSignals.LeftMouseUp(guiEvent.mousePosition, guiEvent);

            if (!IsMouseOverBlock(guiEvent.mousePosition))
            {
                FlowchartWindowSignals.EmptySpaceLeftMouseUp(guiEvent.mousePosition, guiEvent);
            }
        }

        private static bool IsMouseOverBlock(Vector2 mousePosition)
        {
            Flowchart flowchart = EditorSelectionTracker.ActiveFlowchart;
            if (flowchart == null)
            {
                return false;
            }

            float zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 1f : flowchart.Zoom;
            Vector2 mousePosInWindowSpace = mousePosition / zoom;
            Vector2 scrollPos = flowchart.ScrollPos;

            IReadOnlyCollection<Block> blocks = flowchart.Blocks;
            if (blocks == null || blocks.Count == 0)
            {
                Block[] fallback = flowchart.GetComponents<Block>();
                for (int i = 0; i < fallback.Length; i++)
                {
                    if (IsMouseOverBlock(fallback[i], mousePosInWindowSpace, scrollPos))
                    {
                        return true;
                    }
                }

                return false;
            }

            foreach (var block in blocks)
            {
                if (IsMouseOverBlock(block, mousePosInWindowSpace, scrollPos))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsMouseOverBlock(Block block, Vector2 mousePosInWindowSpace, Vector2 scrollPos)
        {
            if (block == null)
            {
                return false;
            }

            Rect windowSpaceRect = block._NodeRect;
            windowSpaceRect.position += scrollPos;

            return windowSpaceRect.Contains(mousePosInWindowSpace);
        }

        private void HandlePanInputRelease(Event guiEvent)
        {
            if (guiEvent.MiddleMouseButton() || guiEvent.RightDragInput())
            {
                activePanAnchor = null;
            }
        }

        private Vector2? activePanAnchor;
        // When the user is panning with middle mouse button (or alt + left), we need to keep 
        // track of the last mouse position to calculate deltas. That last mouse position
        // is stored here.

        private void HandleLeftDragRelease(Event guiEvent)
        {
            if (!isLeftDragActive || !guiEvent.LeftMouseButton() || guiEvent.alt)
            {
                return;
            }

            isLeftDragActive = false;
            FlowchartWindowSignals.LeftMouseDragEnded(guiEvent.mousePosition, guiEvent);
        }

        private void HandleRightDragRelease(Event guiEvent)
        {
            if (!isRightDragActive || !guiEvent.RightMouseButton())
            {
                return;
            }

            isRightDragActive = false;
            FlowchartWindowSignals.RightMouseDragEnded(guiEvent.mousePosition, guiEvent);
        }

        private bool isLeftDragActive;
        private bool isRightDragActive;

        private void HandleMouseDrag(Event guiEvent)
        {
            HandleLeftDrag();
            void HandleLeftDrag()
            {
                if (guiEvent.LeftDragInput())
                {
                    if (!isLeftDragActive)
                    {
                        isLeftDragActive = true;
                        FlowchartWindowSignals.LeftMouseDragStarted(guiEvent.mousePosition, guiEvent);
                        // We don't want to have LeftMouseDragged called on the same
                        // frame as LeftMouseDragStarted, so...
                    }
                    else
                    {
                        FlowchartWindowSignals.LeftMouseDragged(guiEvent.delta, guiEvent);
                    }
                }
            }

            HandleRightDrag();
            void HandleRightDrag()
            {
                if (guiEvent.RightDragInput())
                {
                    if (!isRightDragActive)
                    {
                        isRightDragActive = true;
                        FlowchartWindowSignals.RightMouseDragStarted(guiEvent.mousePosition, guiEvent);
                    }
                    else
                    {
                        FlowchartWindowSignals.RightMouseDragged(guiEvent.delta, guiEvent);
                    }
                }
            }

            if (!activePanAnchor.HasValue || !guiEvent.PanInput())
            {
                Debug.Log("No active pan anchor or no pan input");
                return;
            }

            HandlePanning();
            void HandlePanning()
            {
                Vector2 movementSinceLastFrame = guiEvent.mousePosition - activePanAnchor.Value;
                if (movementSinceLastFrame.sqrMagnitude > Mathf.Epsilon)
                {
                    Debug.Log("Panning");
                    FlowchartWindowSignals.ScrollWheelDragged(movementSinceLastFrame);
                    FlowchartWindowSignals.WindowPanned();
                }

                activePanAnchor = guiEvent.mousePosition;
            }
            
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
            isLeftDragActive = false;
            isRightDragActive = false;
            activePanAnchor = null;
        }
    }
}