using System;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The static signals related to flowcharts.
    /// </summary>
    public static class FlowchartSignals
    {
        public static Action<Flowchart> FlowchartDisabled = delegate { };
        public static Action<Flowchart> FlowchartEnabled = delegate { };
        public static Action<Flowchart> FlowchartDestroyed = delegate { };

        public static Action<Flowchart, IVariable> VariableAdded = delegate { };
        public static Action<Flowchart, IVariable> VariableRemoved = delegate { };
    }

    public interface IVariableAddResponder
    {
        void OnVariableAdded(Flowchart addedTo, IVariable variable);
    }

    public interface IVariableRemoveResponder
    {
        void OnVariableRemoved(Flowchart removedFrom, IVariable variable);
    }
}