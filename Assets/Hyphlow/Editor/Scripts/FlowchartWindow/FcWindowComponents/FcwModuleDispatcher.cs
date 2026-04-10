using System;
using System.Collections;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{

    public abstract class FcwModuleDispatcher : IModuleDispatcher<IFlowchartWindowModule>
    {
        private readonly List<IFlowchartWindowModule> modules = new List<IFlowchartWindowModule>();
        private readonly Dictionary<Type, IList> responderBuckets = new Dictionary<Type, IList>();
        public abstract void ToggleSubs(bool on);


        public virtual void AddModule(object module)
        {
            if (module is not IFlowchartWindowModule flowchartModule)
            {
                throw new ArgumentException($"Module must implement {nameof(IFlowchartWindowModule)}", nameof(module));
            }
            AddModule((IFlowchartWindowModule)module);
        }

        public virtual void RemoveModule(object module)
        {
            if (module is not IFlowchartWindowModule flowchartModule)
            {
                throw new ArgumentException($"Module must implement {nameof(IFlowchartWindowModule)}", nameof(module));
            }

            RemoveModule((IFlowchartWindowModule)module);
        }

        public virtual void AddModule(IFlowchartWindowModule module)
        {
            modules.Add(module);

            AddAsResponder(module);
        }

        protected abstract void AddAsResponder(IFlowchartWindowModule module);
        protected abstract void RemoveAsResponder(IFlowchartWindowModule module);

        public virtual void RemoveModule(IFlowchartWindowModule module)
        {
            modules.Remove(module);
            RemoveAsResponder(module);
        }

        public virtual void ClearModules()
        {
            for (int i = 0; i < modules.Count; i++)
            {
                modules[i].Dispose();
            }

            modules.Clear();
            responderBuckets.Clear();
        }

    }
}