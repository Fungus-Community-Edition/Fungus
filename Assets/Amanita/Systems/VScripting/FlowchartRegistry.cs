using System.Collections.Generic;
using System.Linq;
using System;

namespace Amanita.VScripting
{
    /// <summary>
    /// Can be used as a replacement for Flowchart.CachedFlowcharts.
    /// </summary>
    public class FlowchartRegistry : IDisposable
    {
        // Keys are the guids of the flowcharts
        private readonly IDictionary<string, Flowchart> flowchartLookup = new Dictionary<string, Flowchart>();

        public virtual void Init()
        {
            ToggleSubs(false);
            ToggleSubs(true);
            IsDisposed = false;
        }

        protected virtual void ToggleSubs(bool on)
        {
            if (on)//
            {
                FlowchartSignals.FlowchartEnabled += RegisterFlowchart;
                FlowchartSignals.FlowchartDestroyed += UnregisterFlowchart;
            }
            else
            {
                FlowchartSignals.FlowchartEnabled -= RegisterFlowchart;
                FlowchartSignals.FlowchartDestroyed -= UnregisterFlowchart;
            }
        }

        public void RegisterFlowchart(Flowchart flowchart)
        {
            flowchartLookup[flowchart.UniqueId] = flowchart;
        }

        public void UnregisterFlowchart(Flowchart flowchart)
        {
            flowchartLookup.Remove(flowchart.UniqueId);
        }

        public IReadOnlyList<Flowchart> GetFlowcharts()
        {
            return flowchartLookup.Values.ToList();
        }

        public virtual Flowchart GetFChartWith(string guid)
        {
            flowchartLookup.TryGetValue(guid, out Flowchart flowchart);
            return flowchart;
        }

        public virtual void Clear()
        {
            flowchartLookup.Clear();
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            ToggleSubs(false);
            Clear();
            IsDisposed = true;
        }

        public virtual bool IsDisposed { get; private set; }
    }
}