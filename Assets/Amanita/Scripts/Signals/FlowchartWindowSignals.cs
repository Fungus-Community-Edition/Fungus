using System;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    public static class FlowchartWindowSignals
    {
        public static Action EmptySpaceClicked = delegate { };
        public static Action<Flowchart, Flowchart> ChangedFlowchart = delegate { };
        public static Action<IList<Block>> BlocksCopied = delegate { };
        public static Action<Block> PreBlockDeletion = delegate { };
    }
}