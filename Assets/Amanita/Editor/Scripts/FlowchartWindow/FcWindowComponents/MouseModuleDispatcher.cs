using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public sealed class MouseModuleDispatcher : IModuleDispatcher<IFlowchartWindowModule>
    {
        private readonly List<IFlowchartWindowModule> modules = new List<IFlowchartWindowModule>();
        private readonly Dictionary<Type, IList> responderBuckets = new Dictionary<Type, IList>();

        public void ToggleSubs(bool on)
        {
            if (on)
            {
                FlowchartWindowSignals.LeftClicked += NotifyLeftClick;
                FlowchartWindowSignals.RightClicked += NotifyRightClick;

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

                FlowchartWindowSignals.EmptySpaceClicked += NotifyEmptySpaceClicked;
            }
            else
            {
                FlowchartWindowSignals.LeftClicked -= NotifyLeftClick;
                FlowchartWindowSignals.RightClicked -= NotifyRightClick;

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

                FlowchartWindowSignals.EmptySpaceClicked -= NotifyEmptySpaceClicked;
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
            AddResponder<ILeftClickResponder>(module);
            AddResponder<IRightClickResponder>(module);
            AddResponder<IDoubleClickResponder>(module);

            AddResponder<ILeftMouseDragStartResponder>(module);
            AddResponder<ILeftMouseDragResponder>(module);
            AddResponder<ILeftMouseDragEndResponder>(module);
            AddResponder<ILeftMouseUpResponder>(module);

            AddResponder<IRightMouseDragStartResponder>(module);
            AddResponder<IRightMouseDragResponder>(module);
            AddResponder<IRightMouseDragEndResponder>(module);

            AddResponder<IEmptySpaceClickResponder>(module);
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
            RemoveResponder<ILeftClickResponder>(module);
            RemoveResponder<IRightClickResponder>(module);
            RemoveResponder<IDoubleClickResponder>(module);

            RemoveResponder<ILeftMouseDragStartResponder>(module);
            RemoveResponder<ILeftMouseDragResponder>(module);
            RemoveResponder<ILeftMouseDragEndResponder>(module);
            RemoveResponder<ILeftMouseUpResponder>(module);

            RemoveResponder<IRightMouseDragStartResponder>(module);
            RemoveResponder<IRightMouseDragResponder>(module);
            RemoveResponder<IRightMouseDragEndResponder>(module);

            RemoveResponder<IEmptySpaceClickResponder>(module);
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
        public void NotifyLeftClick(Vector2 position) =>
            Broadcast<ILeftClickResponder>(res => res.OnLeftClick(position));

        public void NotifyRightClick(Vector2 position) =>
            Broadcast<IRightClickResponder>(res => res.OnRightClick(position));

        public void NotifyDoubleClick(Vector2 position) =>
            Broadcast<IDoubleClickResponder>(res => res.OnDoubleClick(position));

        public void NotifyLeftMouseDragStarted(Vector2 startPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragStartResponder>(res => res.OnLeftMouseDragStarted(startPosition, guiEvent));

        public void NotifyLeftMouseDragged(Vector2 currentPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragResponder>(res => res.OnLeftMouseDragged(currentPosition, guiEvent));

        public void NotifyLeftMouseDragEnded(Vector2 endPosition, Event guiEvent) =>
            Broadcast<ILeftMouseDragEndResponder>(res => res.OnLeftMouseDragEnded(endPosition, guiEvent));

        public void NotifyLeftMouseUp(Vector2 position, Event evt) =>
            Broadcast<ILeftMouseUpResponder>(res => res.OnLeftMouseUp(position, evt));

        public void NotifyRightMouseDragStarted(Vector2 startPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragStartResponder>(res => res.OnRightMouseDragStarted(startPosition, guiEvent));

        public void NotifyRightMouseDragged(Vector2 currentPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragResponder>(res => res.OnRightMouseDragged(currentPosition, guiEvent));

        public void NotifyRightMouseDragEnded(Vector2 endPosition, Event guiEvent) =>
            Broadcast<IRightMouseDragEndResponder>(res => res.OnRightMouseDragEnded(endPosition, guiEvent));

        public void NotifyEmptySpaceClicked(Vector2 position) =>
            Broadcast<IEmptySpaceClickResponder>(res => res.OnEmptySpaceClicked(position));

        public void NotifyEmptySpaceLeftMouseDown(Vector2 position, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseDownResponder>(res => res.OnEmptySpaceLeftMouseDown(position, evt));

        public void NotifyEmptySpaceLeftMouseUp(Vector2 position, Event evt) =>
            Broadcast<IEmptySpaceLeftMouseUpResponder>(res => res.OnEmptySpaceLeftMouseUp(position, evt));

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