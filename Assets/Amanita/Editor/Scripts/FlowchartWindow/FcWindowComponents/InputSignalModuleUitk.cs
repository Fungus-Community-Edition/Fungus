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
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindowUitk window)
        {
            RegisterPointerCallbacks(false);
            owner = window != null ? 
                window : 
                throw new ArgumentNullException(nameof(window));
            RegisterPointerCallbacks(true);
            isDisposed = false;
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
                root.RegisterCallback<PointerMoveEvent>(OnPointerMove);
                root.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                root.RegisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
                root.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            }
            else
            {
                root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                root.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                root.UnregisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
                root.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            }
        }

        internal void OnPointerDown(PointerDownEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            SetToImguiEvent(ref _mouseDownEvent, evt, EventType.MouseDown);
            HandleMouseDown(_mouseDownEvent);
        }

        private Event _mouseDownEvent = new Event();

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
        /// Changes the passed guiEvent's fields to match the relevant data from the given IPointerEvent,
        /// and sets the event type to the given EventType.
        /// </summary>
        private void SetToImguiEvent(ref Event guiEvent, IPointerEvent evt, EventType type)
        {
            evt.ApplyButtonTo(guiEvent);
            guiEvent.shift = evt.shiftKey;
            guiEvent.type = type;
            guiEvent.mousePosition = ToFlowchartSpace(evt.position);
            guiEvent.delta = evt.deltaPosition;
            guiEvent.modifiers = GetModifiers(evt);
            guiEvent.clickCount = evt.clickCount;
            guiEvent.alt = evt.altKey;
            
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

        private void SetToImguiEvent(ref Event guiEvent, PointerUpEvent evt, EventType type)
        {
            SetToImguiEvent(ref guiEvent, (IPointerEvent)evt, type);
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

                    if (!BlockHitTester.IsMouseOverBlock(guiEvent.mousePosition))
                    {
                        FlowchartWindowSignals.EmptySpaceLeftMouseDown(guiEvent.mousePosition, guiEvent);
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

        private bool isDisposed;
        private FlowchartWindowUitk owner;

        private static bool IsImGuiPointerEvent(EventType eventType)
        {
            return eventType == EventType.MouseDown
                || eventType == EventType.MouseUp
                || eventType == EventType.MouseDrag
                || eventType == EventType.ScrollWheel;
        }

        internal void OnPointerMove(PointerMoveEvent evt)
        {
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            bool shouldHandleDrag = evt.IsLeftMouseButtonPressed()
                || evt.IsRightMouseButtonPressed()
                || evt.IsMiddleMouseButtonPressed()
                || evt.IsPanInput();

            if (!shouldHandleDrag)
            {
                return;
            }

            Debug.Log("Pointer move handling drag");
            MarkUitkInput();
            SetToImguiEvent(ref _pointerMoveEvent, evt, EventType.MouseDrag);
            HandleMouseDrag(_pointerMoveEvent);
        }

        private Event _pointerMoveEvent = new Event();
        internal void OnPointerUp(PointerUpEvent evt)
        {
            Debug.Log($"Running PointerUp callback with event: {evt}");
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            SetToImguiEvent(ref _pointerUpEvent, evt, EventType.MouseUp);
            HandlePointerRelease(_pointerUpEvent);
        }

        /// <summary>
        /// We need this because Block buttons capture the pointer, keeping OnPointerUp from firing. 
        /// PointerCancelEvent does fire, however, so we can treat it as a pointer up for our purposes.
        /// </summary>
        /// <param name="evt"></param>
        internal void OnPointerCancel(PointerCancelEvent evt)
        {
            Debug.Log($"Running PointerCancel callback with event: {evt}");
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            SetToImguiEvent(ref _pointerUpEvent, evt, EventType.MouseUp);
            HandlePointerRelease(_pointerUpEvent);
        }

        private void HandlePointerRelease(Event guiEvent)
        {
            HandleLeftMouseUp(guiEvent);
            HandlePanInputRelease(guiEvent);
            HandleLeftDragRelease(guiEvent);
            HandleRightDragRelease(guiEvent);
        }

        private Event _pointerUpEvent = new Event();

        private void OnWheel(WheelEvent evt)
        {
            Debug.Log($"Running Wheel callback with event: {evt}");
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            SetToImguiEvent(ref _wheelEvent, evt, EventType.ScrollWheel);
            HandleScrollWheel(_wheelEvent);
        }

        private Event _wheelEvent = new Event();

        private void SetToImguiEvent(ref Event guiEvent, WheelEvent evt, EventType type)
        {
            guiEvent.type = type;
            guiEvent.mousePosition = ToFlowchartSpace(evt.mousePosition);
            guiEvent.delta = evt.delta;
            guiEvent.modifiers = GetModifiers(evt);
            guiEvent.button = evt.button;
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
            Debug.Log($"Mouse drag detected with button: {guiEvent.button}");
            HandleLeftDrag();
            void HandleLeftDrag()
            {
                if (guiEvent.LeftDragInput())
                {
                    if (!isLeftDragActive)
                    {
                        //Debug.Log($"Starting left drag with button: {guiEvent}");
                        isLeftDragActive = true;
                        FlowchartWindowSignals.LeftMouseDragStarted(guiEvent.mousePosition, guiEvent);
                        // We don't want to have LeftMouseDragged called on the same
                        // frame as LeftMouseDragStarted, so...
                    }
                    else
                    {
                        //Debug.Log($"Continuing left drag with event: {guiEvent}");
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

            // We need to reset use uitkinput on layout so that the pointer doesn't get locked
            // on any particular control type. 
            if (guiEvent.type == EventType.Layout)
            {
                useUitkInput = false;
            }

            if (useUitkInput && IsImGuiPointerEvent(guiEvent.type))
            {
                return;
            }

            switch (guiEvent.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown(guiEvent);
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

        private FcWindowGraphicsRendererUitk graphicsRenderer;

        private Vector2 ToFlowchartSpace(Vector2 panelPosition)
        {
            if (graphicsRenderer == null && owner != null)
            {
                graphicsRenderer = owner.rootVisualElement.Q<FcWindowGraphicsRendererUitk>();
            }

            return graphicsRenderer != null
                ? graphicsRenderer.WorldToLocal(panelPosition)
                : panelPosition;
        }
    }
}