using AtMycelia.EditorUtils;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Detects raw IMGUI input inside the UITK flowchart window and relays it to FlowchartWindowSignals.
    /// Call <see cref="OnGUI(Event)"/> from the owning window’s OnGUI loop.
    /// </summary>
    public sealed class InputSignalModule : IFlowchartWindowModule, IDisposable
    {
        public int Priority { get; set; } = 0;
        public void Initialize(FlowchartWindow window)
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
                root.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            }
            else
            {
                root.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                root.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
                root.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
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
            // We want to handle events for one mouse down per frame, which is why when one check
            // succeeds, we skip the others for the rest of the frame. 
            SetPointerEventInfo(ref _mouseDownInfo, guiEvent);
            if (HandleLeftMouseDown(guiEvent))
            {
                return;
            }
            if (HandleRightMouseDown(guiEvent))
            {
                return;
            }
            if (HandleLeftMousePanning(guiEvent))
            {
                return;
            }
        }

        void SetPointerEventInfo(ref PointerEventInfo info, Event guiEvent)
        {
            Vector2 flowchartPos = guiEvent.mousePosition;
            Vector2 panelPos = ToPanelSpace(flowchartPos);
            Vector2 panelDelta = guiEvent.delta;
            Vector2 flowchartDelta = ToFlowchartDelta(panelPos, panelDelta);
            info.FlowchartPosition = flowchartPos;
            info.PanelPosition = panelPos;
            info.FlowchartDelta = flowchartDelta;
            info.PanelDelta = panelDelta;
        }

        private PointerEventInfo _mouseDownInfo;

        private bool HandleLeftMouseDown(Event guiEvent)
        {
            if (guiEvent.LeftMouseButton())
            {
                if (guiEvent.DoubleClick())
                {
                    //Debug.Log("Double click detected");
                    FlowchartWindowSignals.DoubleClicked(_mouseDownInfo);
                }
                else
                {
                    //Debug.Log("Left mouse down detected");
                    FlowchartWindowSignals.LeftMouseDown(_mouseDownInfo);
                    Block blockHit = BlockHitTester.FindTopmostBlock(_mouseDownInfo.PanelPosition);
                    bool mouseOverBlock = blockHit != null;
                    if (!mouseOverBlock)
                    {
                        //Debug.Log("Empty space left mouse down");
                        
                        FlowchartWindowSignals.EmptySpaceLeftMouseDown(_mouseDownInfo, guiEvent);
                    }
                }
            }

            return guiEvent.LeftMouseButton();
        }

        private Vector2 ToPanelSpace(Vector2 flowchartPosition)
        {
            if (graphicsRenderer == null && owner != null)
            {
                graphicsRenderer = owner.rootVisualElement.Q<FcwGraphicsRenderer>();
            }

            if (graphicsRenderer == null)
            {
                return flowchartPosition;
            }

            Vector3 world = graphicsRenderer.worldTransform.MultiplyPoint3x4(flowchartPosition);
            return new Vector2(world.x, world.y);
        }

        private bool HandleRightMouseDown(Event guiEvent)
        {
            if (guiEvent.RightMouseButton())
            {
                //Debug.Log("Right mouse down detected");
                FlowchartWindowSignals.RightMouseDown(_mouseDownInfo);

                bool mouseOverBlock = BlockHitTester.IsMouseOverBlock(this._mouseDownInfo.PanelPosition);
                if (!mouseOverBlock)
                {
                    //Debug.Log("Empty space right mouse down");
                    FlowchartWindowSignals.EmptySpaceRightMouseDown(this._mouseDownInfo, guiEvent);
                }
            }

            return guiEvent.RightClick();
        }

        private bool HandleLeftMousePanning(Event guiEvent)
        {
            if (guiEvent.PanInput())
            {
                //Debug.Log("Pan input started");
                activePanAnchor = guiEvent.mousePosition;
            }

            return guiEvent.PanInput();
        }

        private bool isDisposed;
        private FlowchartWindow owner;

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

            //Debug.Log("Pointer move handling drag");
            MarkUitkInput();
            SetToImguiEvent(ref _pointerMoveEvent, evt, EventType.MouseDrag);
            HandleMouseDrag(_pointerMoveEvent);
        }

        private Event _pointerMoveEvent = new Event();
        internal void OnPointerUp(PointerUpEvent evt)
        {
            //Debug.Log($"Running PointerUp callback with event: {evt}");
            if (!ShouldHandleUiEvent(evt))
            {
                return;
            }

            MarkUitkInput();
            
            SetToImguiEvent(ref _pointerUpEvent, evt, EventType.MouseUp);
            HandlePointerRelease(_pointerUpEvent);
        }

        private PointerEventInfo _pointerUpInfo;

        private void HandlePointerRelease(Event guiEvent)
        {
            SetPointerEventInfo(ref _pointerUpInfo, guiEvent);
            HandleLeftMouseUp(guiEvent);
            HandleRightMouseUp(guiEvent);
            HandlePanInputRelease(guiEvent);
            HandleLeftDragRelease(guiEvent);
            HandleRightDragRelease(guiEvent);
        }

        private Event _pointerUpEvent = new Event();

        private void OnWheel(WheelEvent evt)
        {
            //Debug.Log($"Running Wheel callback with event: {evt}");
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

            FlowchartWindowSignals.LeftMouseUp(_pointerUpInfo, guiEvent);

            if (!IsMouseOverBlock(_pointerUpInfo.PanelPosition))
            {
                FlowchartWindowSignals.EmptySpaceLeftMouseUp(_pointerUpInfo, guiEvent);
                if (owner.FcContext.Interaction.BlockHitInLastMouseDown == null)
                {
                    Debug.Log("Empty space left-clicked");
                    FlowchartWindowSignals.EmptySpaceLeftClicked(_pointerUpInfo);
                }
            }
        }

        private void HandleRightMouseUp(Event guiEvent)
        {
            if (!guiEvent.RightMouseButton())
            {
                return;
            }

            FlowchartWindowSignals.RightMouseUp(_pointerUpInfo, guiEvent);
            Block blockHit = BlockHitTester.FindTopmostBlock(_pointerUpInfo.PanelPosition);
            if (blockHit == null)
            {
                FlowchartWindowSignals.EmptySpaceRightMouseUp(_pointerUpInfo, guiEvent);

                if (owner.FcContext.Interaction.BlockHitInLastMouseDown == null)
                {
                    Debug.Log("Empty space right-clicked");
                    FlowchartWindowSignals.EmptySpaceRightClicked(_pointerUpInfo);
                }
            }
            else
            {
                Debug.Log($"Right-clicked on block: {blockHit.BlockName}");
                BlockSignals.BlockRightClicked(blockHit, _pointerUpInfo);
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
            FlowchartWindowSignals.LeftMouseDragEnded(_pointerUpInfo, guiEvent);
        }

        private void HandleRightDragRelease(Event guiEvent)
        {
            if (!isRightDragActive || !guiEvent.RightMouseButton())
            {
                return;
            }

            isRightDragActive = false;
            FlowchartWindowSignals.RightMouseDragEnded(_pointerUpInfo, guiEvent);
        }

        private bool isLeftDragActive;
        private bool isRightDragActive;

        private void HandleMouseDrag(Event guiEvent)
        {
            //Debug.Log($"Mouse drag detected with button: {guiEvent.button}");
            SetPointerEventInfo(ref _mouseDragInfo, guiEvent);
            HandleLeftDrag();
            void HandleLeftDrag()
            {
                if (guiEvent.LeftDragInput())
                {
                    if (!isLeftDragActive)
                    {
                        isLeftDragActive = true;
                        FlowchartWindowSignals.LeftMouseDragStarted(_mouseDragInfo, guiEvent);
                    }
                    else
                    {
                        FlowchartWindowSignals.LeftMouseDragged(_mouseDragInfo, guiEvent);
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
                        FlowchartWindowSignals.RightMouseDragStarted(_mouseDragInfo, guiEvent);
                    }
                    else
                    {
                        FlowchartWindowSignals.RightMouseDragged(_mouseDragInfo, guiEvent);
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
                    //Debug.Log("Panning");
                    FlowchartWindowSignals.ScrollWheelDragged(movementSinceLastFrame);
                    FlowchartWindowSignals.WindowPanned();
                }

                activePanAnchor = guiEvent.mousePosition;
            }

            guiEvent.Use();
        }

        private PointerEventInfo _mouseDragInfo;

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

            // We need to reset use uitkInput on layout so that the pointer doesn't get locked
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

        private FcwGraphicsRenderer graphicsRenderer;

        private Vector2 ToFlowchartSpace(Vector2 panelPosition)
        {
            if (graphicsRenderer == null && owner != null)
            {
                graphicsRenderer = owner.rootVisualElement.Q<FcwGraphicsRenderer>();
            }

            return graphicsRenderer != null
                ? graphicsRenderer.WorldToLocal(panelPosition)
                : panelPosition;
        }

        private Vector2 ToFlowchartDelta(Vector2 panelPosition, Vector2 panelDelta)
        {
            Vector2 start = ToFlowchartSpace(panelPosition);
            Vector2 end = ToFlowchartSpace(panelPosition + panelDelta);
            return end - start;
        }

    }
}