using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class ConnectionGatherer : IConnectionGatherer
    {
        public virtual IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx)
        {
            FlowchartContext fcContext = drawCtx.FlowchartCtx;
            var fc = fcContext.Flowchart;
            var viewRect = drawCtx.ViewRect;
            var result = new List<ConnectionInfo>();

            // 1. collect valid blocks
            var blocks = fcContext.AllBlocks
                .Where(elem => elem != null)
                .ToList();

            foreach (var blockEl in blocks)
            {
                bool blockIsSelected = (fc.SelectedBlock == blockEl);
                var fromBase = blockEl._NodeRect;
                var validCommands = blockEl.CommandList.Where(elem => elem != null);

                // 2. for each command, resolve connected blocks
                foreach (var commandEl in validCommands)
                {
                    bool cmdIsSelected = fc.SelectedCommands.Contains(commandEl);
                    bool shouldHighlight = commandEl.IsExecuting || (blockIsSelected && cmdIsSelected);
                    connectedBlocks.Clear();
                    commandEl.GetConnectedBlocks(ref connectedBlocks);

                    foreach (var dest in connectedBlocks)
                    {
                        // We only want to consider blocks that are NOT:
                        // 1. null
                        // 2. the same one as the source
                        // 3. in a different Flowchart
                        if (dest == null || dest == blockEl || dest.GetFlowchart() != fc)
                            continue;

                        // 3. adjust for pan/zoom
                        var fromScrolled = ScrollRect(fromBase, fc);
                        var toScrolled = ScrollRect(dest._NodeRect, fc);

                        // 4. cull by view
                        if (OverlapsViewport(fromScrolled, toScrolled, viewRect))
                            result.Add(new ConnectionInfo(fromScrolled, toScrolled, shouldHighlight));
                    }
                }
            }

            return result;
        }

        // Keeping things all in one list for performance reasons
        protected List<Block> connectedBlocks = new List<Block>();

        static Rect ScrollRect(Rect r, Flowchart fc)
        {
            r.x += fc.ScrollPos.x;
            r.y += fc.ScrollPos.y;
            return r;
        }

        static bool OverlapsViewport(Rect a, Rect b, Rect view)
        {
            var bound = Rect.MinMaxRect(
                Mathf.Min(a.xMin, b.xMin),
                Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax),
                Mathf.Max(a.yMax, b.yMax));
            return bound.Overlaps(view);
        }

        public virtual void Dispose()
        {
            connectedBlocks.Clear();
            connectedBlocks = null;
        }
    }
}
