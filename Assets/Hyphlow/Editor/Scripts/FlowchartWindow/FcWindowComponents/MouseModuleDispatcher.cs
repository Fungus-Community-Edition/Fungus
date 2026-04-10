using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public sealed class MouseModuleDispatcher : IModuleDispatcher<IFlowchartWindowModule>
    {
        private readonly List<IFlowchartWindowModule> modules = new List<IFlowchartWindowModule>();
        private readonly Dictionary<Type, IList> responderBuckets = new Dictionary<Type, IList>();

        public void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartWindowSignals.LeftMouseDown += NotifyLeftMouseDown;
                FlowchartWindowSignals.RightMouseDown += NotifyRightClick;

                FlowchartWindowSignals.LeftMouseUp += NotifyLeftMouseUp;
                FlowchartWindowSignals.EmptySpaceLeftMouseDown += NotifyEmptySpaceLeftMouseDown;
                FlowchartWindowSignals.EmptySpaceLeftMouseUp += NotifyEmptySpaceLeftMouseUp;

                FlowchartWindowSignals.LeftMouseDragStarted += NotifyLeftMouseDragStarted;
                FlowchartWindowSignals.LeftMouseDragged += NotifyLeftMouseDragged;
                FlowchartWindowSignals.LeftMouseDragEnded += NotifyLeftMouseDragEnded;

                FlowchartWindowSignals.RightMouseDragStarted += NotifyRightMouseDragStarted;
                FlowchartWindowSignals.RightMouseDragged += NotifyRightMouseDragged;
                FlowchartWindowSignals.RightMouseDragEnded += NotifyRightMouseDragEnded;

                FlowchartWindowSignals.DoubleClicked += NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved += NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged += NotifyScrollWheelDragged;

                FlowchartWindowSignals.EmptySpaceLeftClicked += NotifyEmptySpaceLeftClicked;
            }
            else
            {
                FlowchartWindowSignals.LeftMouseDown -= NotifyLeftMouseDown;
                FlowchartWindowSignals.RightMouseDown -= NotifyRightClick;

                FlowchartWindowSignals.LeftMouseUp -= NotifyLeftMouseUp;
                FlowchartWindowSignals.EmptySpaceLeftMouseDown -= NotifyEmptySpaceLeftMouseDown;
                FlowchartWindowSignals.EmptySpaceLeftMouseUp -= NotifyEmptySpaceLeftMouseUp;

                FlowchartWindowSignals.LeftMouseDragStarted -= NotifyLeftMouseDragStarted;
                FlowchartWindowSignals.LeftMouseDragged -= NotifyLeftMouseDragged;
                FlowchartWindowSignals.LeftMouseDragEnded -= NotifyLeftMouseDragEnded;

                FlowchartWindowSignals.RightMouseDragStarted -= NotifyRightMouseDragStarted;
                FlowchartWindowSignals.RightMouseDragged -= NotifyRightMouseDragged;
                FlowchartWindowSignals.RightMouseDragEnded -= NotifyRightMouseDragEnded;

                FlowchartWindowSignals.DoubleClicked -= NotifyDoubleClick;
                FlowchartWindowSignals.ScrollWheelMoved -= NotifyScrollWheelMoved;
                FlowchartWindowSignals.ScrollWheelDragged -= NotifyScrollWheelDragged;

                FlowchartWindowSignals.EmptySpaceLeftClicked -= NotifyEmptySpaceLeftClicked;
            }
        }

        public void AddModule(object module)
        {
            if (module is not IFlowchartWindowModule flowchartModule)
            {
                throw new ArgumentException($"Module must implement {nameof(IFlowchartWindowModule)}", nameof(module));
            }
            AddModule((IFlowchartWindowModule)module);
        }

        public void RemoveModule(object module)
        {
            if (module is not IFlowchartWindowModule flowchartModule)
            {
                throw new ArgumentException($"Module must implement {nameof(IFlowchartWindowModule)}", nameof(module));
            }

            RemoveModule((IFlowchartWindowModule)module);
        }

        public void AddModule(IFlowchartWindowModule module)
        {
            modules.Add(module);

            #region Mouse Events
            AddResponder<ILeftMouseDownResponder>(module);
            AddResponder<IRightClickResponder>(module);
            AddResponder<IDoubleClickResponder>(module);

            AddResponder<ILeftMouseDragStartResponder>(module);
            AddResponder<ILeftMouseDragResponder>(module);
            AddResponder<ILeftMouseDragEndResponder>(module);
            AddResponder<ILeftMouseUpResponder>(module);

            AddResponder<IRightMouseDragStartResponder>(module);
            AddResponder<IRightMouseDragResponder>(module);
            AddResponder<IRightMouseDragEndResponder>(module);

            AddResponder<IEmptySpaceLeftClickResponder>(module);
            AddResponder<IEmptySpaceLeftMouseDownResponder>(module);
            AddResponder<IEmptySpaceLeftMouseUpResponder>(module);

            AddResponder<IScrollWheelMoveResponder>(module);
            AddResponder<IScrollWheelDragResponder>(module);
            #endregion
        }

        public void RemoveModule(IFlowchartWindowModule module)
        {
            modules.Remove(module);

            #region Mouse Events
            RemoveResponder<ILeftMouseDownResponder>(module);
            RemoveResponder<IRightClickResponder>(module);
            RemoveResponder<IDoubleClickResponder>(module);

            RemoveResponder<ILeftMouseDragStartResponder>(module);
            RemoveResponder<ILeftMouseDragResponder>(module);
            RemoveResponder<ILeftMouseDragEndResponder>(module);
            RemoveResponder<ILeftMouseUpResponder>(module);

            RemoveResponder<IRightMouseDragStartResponder>(module);
            RemoveResponder<IRightMouseDragResponder>(module);
            RemoveResponder<IRightMouseDragEndResponder>(module);

            RemoveResponder<IEmptySpaceLeftClickResponder>(module);
            RemoveResponder<IEmptySpaceLeftMouseDownResponder>(module);
            RemoveResponder<IEmptySpaceLeftMouseUpResponder>(module);

            RemoveResponder<IScrollWheelMoveResponder>(module);
            RemoveResponder<IScrollWheelDragResponder>(module);
            #endregion
        }

        public void ClearModules()
        {
            modules.Clear();
            responderBuckets.Clear();
        }

        #region Notifiers

        #region Mouse Notifiers

        public void NotifyLeftMouseDown(PointerEventInfo info) =>
            Broadcast<ILeftMouseDownResponder>(res => res.OnLeftMouseDown(info));

        public void NotifyRightClick(PointerEventInfo info) =>
            Broadcast<IRightClickResponder>(res => res.OnRightClick(info));

        public void NotifyDoubleClick(PointerEventInfo info) =>
            Broadcast<IDoubleClickResponder>(res => res.OnDoubleClick(info));

        public void NotifyLeftMouseDragStarted(PointerEventInfo info, Event guiEvent) =>
            Broadcast<ILeftMouseDragStartResponder>(res => res.OnLeftMouseDragStarted(info, guiEvent));

        public void NotifyLeftMouseDragged(PointerEventInfo info, Event guiEvent) =>
            Broadcast<ILeftMouseDragResponder>(res => res.OnLeftMouseDragged(info, guiEvent));

        public void NotifyLeftMouseDragEnded(PointerEventInfo info, Event guiEvent) =>
            Broadcast<ILeftMouseDragEndResponder>(res => res.OnLeftMouseDragEnded(info, guiEvent));

        public void NotifyLeftMouseUp(PointerEventInfo info, Event evt) =>
            Broadcast<ILeftMouseUpResponder>(res => res.OnLeftMouseUp(info, evt));

        public void NotifyRightMouseDragStarted(PointerEventInfo info, Event guiEvent) =>
            Broadcast<IRightMouseDragStartResponder>(res => res.OnRightMouseDragStarted(info, guiEvent));

        public void NotifyRightMouseDragged(PointerEventInfo info, Event guiEvent) =>
            Broadcast<IRightMouseDragResponder>(res => res.OnRightMouseDragged(info, guiEvent));

        public void NotifyRightMouseDragEnded(PointerEventInfo info, Event guiEvent) =>
            Broadcast<IRightMouseDragEndResponder>(res => res.OnRightMouseDragEnded(info, guiEvent));

        public void NotifyEmptySpaceLeftClicked(PointerEventInfo info) =>
            Broadcast<IEmptySpaceLeftClickResponder>(res => res.OnEmptySpaceLeftClicked(info));

        public void NotifyEmptySpaceLeftMouseDown(PointerEventInfo info, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseDownResponder>(res => res.OnEmptySpaceLeftMouseDown(info, evt));

        public void NotifyEmptySpaceLeftMouseUp(PointerEventInfo info, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseUpResponder>(res => res.OnEmptySpaceLeftMouseUp(info, evt));

        public void NotifyScrollWheelMoved() =>
            Broadcast<IScrollWheelMoveResponder>(res => res.OnScrollWheelMoved());

        public void NotifyScrollWheelDragged(Vector2 direction) =>
            Broadcast<IScrollWheelDragResponder>(res => res.OnScrollWheelDragged(direction));
        #endregion

        #endregion

        private void AddResponder<TResponder>(IFlowchartWindowModule module)
            where TResponder : class
        {
            if (module is not TResponder responder)
            {
                return;
            }

            List<TResponder> bucket = GetOrCreateBucket<TResponder>();
            bucket.Add(responder);
        }

        private void RemoveResponder<TResponder>(IFlowchartWindowModule module)
            where TResponder : class
        {
            if (module is not TResponder responder)
            {
                return;
            }

            Type key = typeof(TResponder);
            if (!responderBuckets.TryGetValue(key, out IList bucket))
            {
                return;
            }

            List<TResponder> typedBucket = (List<TResponder>)bucket;
            typedBucket.Remove(responder);

            if (typedBucket.Count == 0)
            {
                responderBuckets.Remove(key);
            }
        }

        private List<TResponder> GetOrCreateBucket<TResponder>()
            where TResponder : class
        {
            Type key = typeof(TResponder);
            if (!responderBuckets.TryGetValue(key, out IList bucket))
            {
                var newBucket = new List<TResponder>();
                responderBuckets[key] = newBucket;
                return newBucket;
            }

            return (List<TResponder>)bucket;
        }

        private void Broadcast<TResponder>(Action<TResponder> action)
            where TResponder : class
        {
            if (!responderBuckets.TryGetValue(typeof(TResponder), out IList bucket))
            {
                return;
            }

            List<TResponder> typedBucket = (List<TResponder>)bucket;
            for (int i = 0; i < typedBucket.Count; i++)
            {
                action(typedBucket[i]);
            }
        }
    }
}