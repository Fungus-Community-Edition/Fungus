using System;
using System.Collections;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public interface IFlowchartWindowModule : IDisposable
    {
        /// <summary>
        /// Lower number, sooner execution; Modules are executed in ascending order of this value.
        /// </summary>
        int Priority { get; set; }
        void Initialize(FlowchartWindow window);
    }

    /// <summary>
    /// Dispatcher for events related to the flowchart as a whole, such as flowchart changes, 
    /// command selection, and window panning.
    /// </summary>
    public sealed class FlowchartModuleDispatcher
    {
        private readonly List<IFlowchartWindowModule> modules = new List<IFlowchartWindowModule>();
        private readonly Dictionary<Type, IList> responderBuckets = new Dictionary<Type, IList>();

        public void AddModule(IFlowchartWindowModule module)
        {
            modules.Add(module);

            AddResponder<IFlowchartChangeResponder>(module);
            AddResponder<ICommandSelectionResponder>(module);
            AddResponder<IWindowPanResponder>(module);

            AddResponder<IVariableAddResponder>(module);
            AddResponder<IVariableRemoveResponder>(module);
        }

        public void RemoveModule(IFlowchartWindowModule module)
        {
            modules.Remove(module);

            RemoveResponder<IFlowchartChangeResponder>(module);

            RemoveResponder<ICommandSelectionResponder>(module);

            RemoveResponder<IWindowPanResponder>(module);

            RemoveResponder<IVariableAddResponder>(module);
            RemoveResponder<IVariableRemoveResponder>(module);

        }

        public void ClearModules()
        {
            modules.Clear();
            responderBuckets.Clear();
        }

        #region Notifiers

        public void NotifyFlowchartChanged(Flowchart prev, Flowchart current) =>
            Broadcast<IFlowchartChangeResponder>(res => res.OnFlowchartChanged(prev, current));

        public void NotifyCommandSelected(Command command) =>
            Broadcast<ICommandSelectionResponder>(res => res.OnCommandSelected(command));

        public void NotifyWindowPanned() =>
            Broadcast<IWindowPanResponder>(res => res.OnWindowPanned());

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