using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class ConnectionGatherer : IConnectionGatherer
    {
        private const float BlockNamePadding = 10f;
        private List<Block> connectedBlocks = new List<Block>();

        public virtual IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx)
        {
            var fcContext = drawCtx.FlowchartCtx;
            var fc = fcContext.Flowchart;
            var viewRect = drawCtx.ViewRect;
            var result = new List<ConnectionInfo>();
            var document = fcContext.Document;

            foreach (var blockEl in document.AllBlocks.Where(b => b != null))
            {
                bool blockIsSelected = fc.SelectedBlock == blockEl;
                Rect fromRect = CalculateWindowRect(blockEl, drawCtx, fc);

                foreach (var commandEl in blockEl.CommandList.Where(cmd => cmd != null))
                {
                    bool cmdIsSelected = fc.SelectedCommands.Contains(commandEl);
                    bool shouldHighlight = commandEl.IsExecuting || (blockIsSelected && cmdIsSelected);

                    connectedBlocks.Clear();
                    commandEl.GetConnectedBlocks(ref connectedBlocks);

                    foreach (var dest in connectedBlocks)
                    {
                        if (dest == null || dest == blockEl || dest.GetFlowchart() != fc)
                            continue;

                        Rect toRect = CalculateWindowRect(dest, drawCtx, fc);
                        if (OverlapsViewport(fromRect, toRect, viewRect))
                        {
                            result.Add(new ConnectionInfo(fromRect, toRect, shouldHighlight));
                        }
                    }
                }
            }

            return result;
        }

        private static Rect CalculateWindowRect(Block block, DrawBlockContext drawCtx, Flowchart fc)
        {
            Rect modelRect = block._NodeRect;
            GUIStyle nodeStyle = drawCtx.NodeStyle ?? GUI.skin.label;
            Vector2 textSize = nodeStyle.CalcSize(new GUIContent(block.BlockName));

            modelRect.width = Mathf.Clamp(textSize.x + BlockNamePadding, drawCtx.BlockMinWidth, drawCtx.BlockMaxWidth);
            modelRect.height = drawCtx.DefaultBlockHeight;

            if (drawCtx.UseGridSnap)
                modelRect = modelRect.SnapPosition(drawCtx.GridObjectSnap);

            if (fc != null)
                modelRect.position += fc.ScrollPos;

            return modelRect;
        }

        private static bool OverlapsViewport(Rect a, Rect b, Rect view)
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
        }
    }
}
