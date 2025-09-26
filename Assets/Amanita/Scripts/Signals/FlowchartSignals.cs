using System;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    public static class FlowchartSignals
    {
        public static Action<Flowchart, IList<Block>> BlockSelectionCleared = delegate { };
    }
}