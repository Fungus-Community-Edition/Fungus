using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public interface IBlockRectProvider
    {
        bool TryGetBlockRect(Block block, out Rect rect);
    }

    /// <summary>
    /// Gathers connection information for all blocks in the flowchart, including 
    /// their screen-space rectangles and highlight status.
    /// </summary>
    public sealed class ConnectionGatherer : IConnectionGatherer
    {
        private const bool DiagnosticsEnabled = true;

        private List<Block> connectedBlocks = new List<Block>();
        private readonly IBlockRectProvider rectProvider;

        public ConnectionGatherer(IBlockRectProvider rectProvider)
        {
            this.rectProvider = rectProvider;
        }

        public IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx)
        {
            var fcContext = drawCtx.FlowchartCtx;
            var fc = fcContext.Flowchart;
            var viewRect = drawCtx.ViewRect;
            var result = new List<ConnectionInfo>();
            var document = fcContext.Document;

            foreach (Block blockEl in document.AllBlocks)
            {
                if (blockEl == null)
                {
                    continue;
                }

                bool blockIsSelected = fc.SelectedBlock == blockEl;
                Rect fromRect = CalculateWindowRect(blockEl, fc);

                var commands = blockEl.CommandList;
                for (int i = 0; i < commands.Count; i++)
                {
                    Command commandEl = commands[i];
                    if (commandEl == null)
                    {
                        continue;
                    }

                    bool cmdIsSelected = fc.SelectedCommands.Contains(commandEl);
                    bool shouldHighlight = commandEl.IsExecuting || (blockIsSelected && cmdIsSelected);

                    connectedBlocks.Clear();
                    commandEl.GetConnectedBlocks(ref connectedBlocks);

                    for (int j = 0; j < connectedBlocks.Count; j++)
                    {
                        Block dest = connectedBlocks[j];
                        if (dest == null || dest == blockEl || dest.GetFlowchart() != fc)
                        {
                            continue;
                        }

                        Rect toRect = CalculateWindowRect(dest, fc);
                        if (OverlapsViewport(fromRect, toRect, viewRect))
                        {
                            result.Add(new ConnectionInfo(blockEl, dest, shouldHighlight));
                        }
                        else if (DiagnosticsEnabled)
                        {
                            //Debug.Log($"[ConnectionGathererUitk] Skip connection. From={fromRect} To={toRect} View={viewRect}");
                        }
                    }
                }
            }

            return result;
        }

        private Rect CalculateWindowRect(Block block, Flowchart fc)
        {
            if (rectProvider != null && rectProvider.TryGetBlockRect(block, out Rect rect))
            {
                return rect;
            }

            Rect modelRect = block._NodeRect;

            float zoom = 1f;
            Vector2 scrollPos = Vector2.zero;
            if (fc != null)
            {
                zoom = Mathf.Approximately(fc.Zoom, 0f) ? 1f : fc.Zoom;
                scrollPos = fc.ScrollPos;
            }

            modelRect.width *= zoom;
            modelRect.height *= zoom;
            modelRect.position = (modelRect.position + scrollPos) * zoom;
            return modelRect;
        }

        private static bool OverlapsViewport(Rect a, Rect b, Rect view)
        {
            var bound = Rect.MinMaxRect(
                Mathf.Min(a.xMin, b.xMin),
                Mathf.Min(a.yMin, b.yMin),
                Mathf.Max(a.xMax, b.xMax),
                Mathf.Max(a.yMax, b.yMax));

            if (DiagnosticsEnabled && !bound.Overlaps(view))
            {
                //Debug.Log($"[ConnectionGathererUitk] Bound={bound} does not overlap View={view}");
            }

            return bound.Overlaps(view);
        }

        public void Dispose()
        {
            connectedBlocks.Clear();
        }
    }
}