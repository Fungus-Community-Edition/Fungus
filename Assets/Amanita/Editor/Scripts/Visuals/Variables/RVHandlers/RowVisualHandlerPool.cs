using System;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class RowVisualHandlerPool
    {
        public RowVisualHandlerPool
            (
            IRowVisualHandlerResolver handlerResolver,
            IDictionary<Type, Type> visualHandlerLookup
            )
        {
            _handlerResolver = handlerResolver;
            _visualHandlerLookup = visualHandlerLookup;
        }

        protected readonly IRowVisualHandlerResolver _handlerResolver;

        // Again: the keys are the types the vars hold (string, int, etc) while the values
        // are the types of the visual handlers
        protected readonly IDictionary<Type, Type> _visualHandlerLookup;

        public IRowVisualHandler GetHandlerFor(Type contentType, IVariable variable)
        {
            IRowVisualHandler result;
            // Figure out which handler *class* we need
            Type handlerType = _handlerResolver.ResolveHandler(_visualHandlerLookup, contentType);

            bool recyclableInstanceAvailable = _poolMap.TryGetValue(handlerType, out var stack) && stack.Count > 0;
            if (recyclableInstanceAvailable)
            {
                result = stack.Pop();
            }
            else
            {
                result = (IRowVisualHandler)Activator.CreateInstance(handlerType);
            }

            result.Init(variable);
            return result;
        }

        protected readonly Dictionary<Type, Stack<IRowVisualHandler>> _poolMap
            = new Dictionary<Type, Stack<IRowVisualHandler>>();

        #region Just for testing
        /// <summary>
        /// How many handlers (of any particular IRowVisualHandler implementation)
        /// are in the pool at this time.
        /// </summary>
        public virtual int PooledHandlerCount
        {
            get
            {
                int result = 0;
                foreach (var key in _poolMap.Keys)
                {
                    var handlerHolder = _poolMap[key];
                    result += handlerHolder.Count;
                }

                return result;
            }
        }

        public Dictionary<Type, Stack<IRowVisualHandler>> PoolMap => _poolMap;

        public virtual void Clear()
        {
            foreach (var stackEl in _poolMap.Values)
            {
                stackEl.Clear();
            }
        }

        #endregion

        public virtual void ReleaseRange(IEnumerable<IRowVisualHandler> toRelease)
        {
            foreach (var elem in toRelease)
            {
                Release(elem);
            }
        }
        public void Release(IRowVisualHandler handler)
        {
            PrepHandlerForReuse();
            void PrepHandlerForReuse()
            {
                if (handler is IResettable resettable)
                {
                    resettable.Reset();
                }
                else
                {
                    handler.Dispose();
                }
            }

            Type handlerType = handler.GetType();
            bool haveStackReadyForThisType = _poolMap.TryGetValue(handlerType, out var stack);
            if (!haveStackReadyForThisType)
            {
                stack = new Stack<IRowVisualHandler>();
                _poolMap[handlerType] = stack;
                // Having things in a stack reduces lookup time from O(n) to O(1),
                // since you'd be setting up dedicated containers for copies of
                // whatever kind of instance you're looking for
            }

            if (!stack.Contains(handler))
            {
                stack.Push(handler);
            }
        }

        public virtual void ReleaseIn(IEnumerable<VariableRow> rows)
        {
            foreach (var row in rows)
            {
                Release(row.VisualHandler);
            }
        }

        [Conditional("DEV_DIAGNOSTICS")]
        public void DumpState()
        {
            foreach (var kvp in _poolMap)
                Debug.Log($"{kvp.Key.Name}: {kvp.Value.Count} in pool");
        }
    }

}