using Amanita.EditorUtils;
using System;
using UnityEngine;
using UnityEngine.UIElements;
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
            owner = window != null ? 
                window : 
                throw new ArgumentNullException(nameof(window));
            RegisterPointerCallbacks(true);
        }

        private void RegisterPointerCallbacks(bool on)
        {
            if (owner == null)
            {
                return;
            }

            VisualElement root = owner.rootVisualElement;
            if (on)
            {
                root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                root.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
                root.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                root.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            }
            else
            {
                root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                root.UnregisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
                root.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                root.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            }
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            Event guiEvent = ToImguiEvent(evt, EventType.MouseDown);

            HandleMouseDown(guiEvent);
            HandleLeftMouseDown(guiEvent);
        }

        private bool ShouldHandleUiEvent(EventBase evt)
        {
            if (isDisposed || owner == null || evt == null)
            {
                return false;
            }

            return true;
        }

        private void MarkUitkInput()
        {
            useUitkInput = true;
        }
        private bool useUitkInput;
        // ^When true, OnGUI (an IMGUI function) will do nothing so we don't respond to input events
        // more times per frame than intended.

        /// <summary>
        /// Converts a PointerDownEvent to an IMGUI Event. We do this to reuse existing
        /// IMGUI-based input handling code, reducing logic-duplication.
        /// </summary>
        private static Event ToImguiEvent(PointerDownEvent evt, EventType type)
        {
            Event guiEvent = ToImguiEvent((IPointerEvent)evt, type);
            guiEvent.clickCount = evt.clickCount;
            return guiEvent;
        }

        private static Event ToImguiEvent(IPointerEvent evt, EventType type)
        {
            return new Event
            {
                type = type,
                button = evt.button,
                mousePosition = evt.position,
                delta = evt.deltaPosition,
                modifiers = GetModifiers(evt)
            };
        }

        private static EventModifiers GetModifiers(IPointerEvent evt)
        {
            EventModifiers modifiers = EventModifiers.None;

            if (evt.altKey)
            {
                modifiers |= EventModifiers.Alt;
            }

            if (evt.ctrlKey)
            {
                modifiers |= EventModifiers.Control;
            }

            if (evt.shiftKey)
            {
                modifiers |= EventModifiers.Shift;
            }

            if (evt.commandKey)
            {
                modifiers |= EventModifiers.Command;
            }

            return modifiers;
        }

        private static Event ToImguiEvent(PointerUpEvent evt, EventType type)
        {
            Event guiEvent = ToImguiEvent((IPointerEvent)evt, type);
            guiEvent.clickCount = evt.clickCount;
            return guiEvent;
        }

        private static EventModifiers GetModifiers(IMouseEvent evt)
        {
            EventModifiers modifiers = EventModifiers.None;

            if (evt.altKey)
            {
                modifiers |= EventModifiers.Alt;
            }

            if (evt.ctrlKey)
            {
                modifiers |= EventModifiers.Control;
            }

            if (evt.shiftKey)
            {
                modifiers |= EventModifiers.Shift;
            }

            if (evt.commandKey)
            {
                modifiers |= EventModifiers.Command;
            }

            return modifiers;
        }

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
                    bool mouseOverBlock = BlockHitTester.IsMouseOverBlock(guiEvent.mousePosition);
                    if (!mouseOverBlock)
                    {
                        Debug.Log("Empty space clicked");
                        FlowchartWindowSignals.EmptySpaceClicked(guiEvent.mousePosition);
                    }
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

            if (!BlockHitTester.IsMouseOverBlock(guiEvent.mousePosition))
            {
                FlowchartWindowSignals.EmptySpaceLeftMouseDown(guiEvent.mousePosition, guiEvent);
            }
        }

        private bool isDisposed;
        private FlowchartWindowUitk owner;

        private static bool IsImGuiPointerEvent(EventType eventType)
        {
            return eventType == EventType.MouseDown
                || eventType == EventType.MouseUp
                || eventType == EventType.MouseDrag
                || eventType == EventType.ScrollWheel;
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            Event guiEvent = ToImguiEvent(evt, EventType.MouseDrag);

            HandleLeftDrag();
            void HandleLeftDrag()
            {
                if (evt.IsLeftMouseButtonPressed() && !evt.altKey)
                {
                    if (!isLeftDragActive)
                    {
                        isLeftDragActive = true;
                        FlowchartWindowSignals.LeftMouseDragStarted(evt.position, guiEvent);
                    }
                    else
                    {
                        FlowchartWindowSignals.LeftMouseDragged(evt.deltaPosition, guiEvent);
                    }
                }
            }

            HandleRightDrag();
            void HandleRightDrag()
            {
                if (evt.IsRightMouseButtonPressed())
                {
                    if (!isRightDragActive)
                    {
                        isRightDragActive = true;
                        FlowchartWindowSignals.RightMouseDragStarted(evt.position, guiEvent);
                    }
                    else
                    {
                        FlowchartWindowSignals.RightMouseDragged(evt.deltaPosition, guiEvent);
                    }
                }
            }

            HandlePanning();
            void HandlePanning()
            {
                if (activePanAnchor.HasValue && evt.IsPanInput())
                {
                    Vector2 movementSinceLastFrame = (Vector2)evt.position - activePanAnchor.Value;
                    bool movedFarEnough = movementSinceLastFrame.sqrMagnitude > Mathf.Epsilon;
                    if (movedFarEnough)
                    {
                        FlowchartWindowSignals.ScrollWheelDragged(movementSinceLastFrame);
                        FlowchartWindowSignals.WindowPanned();
                    }

                    activePanAnchor = evt.position;
                }
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            Event guiEvent = ToImguiEvent(evt, EventType.MouseUp);

            HandleLeftMouseUp(guiEvent);
            HandlePanInputRelease(guiEvent);
            HandleLeftDragRelease(guiEvent);
            HandleRightDragRelease(guiEvent);
        }

        private void OnWheel(WheelEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            var guiEvent = ToImguiEvent(evt);
            HandleScrollWheel(guiEvent);
        }

        private static Event ToImguiEvent(WheelEvent evt)
        {
            return new Event
            {
                type = EventType.ScrollWheel,
                mousePosition = evt.mousePosition,
                delta = evt.delta,
                modifiers = GetModifiers(evt)
            };
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
            return BlockHitTester.IsMouseOverBlock(mousePosition);
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
            //Debug.Log("Mouse drag detected");
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
            RegisterPointerCallbacks(false);
            owner = null;
        }

        public void OnGUI(Event guiEvent)
        {
            if (isDisposed || guiEvent == null)
            {
                return;
            }

            if (useUitkInput && IsImGuiPointerEvent(guiEvent.type))
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

    }
}