using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UIElements.VisualElement;
using UitkButton = UnityEngine.UIElements.Button;

namespace Amanita.VScripting.EditorUtils.FcWindow
{
    public interface IBlockRectProvider
    {
        bool TryGetBlockRect(Block block, out Rect rect);
    }

    public sealed class ConnectionGatherer : IConnectionGatherer
    {
        private const float PaddingX = 18f;
        private const float PaddingY = 10f;
        private const int BaseFontSize = 12;
        private const float MinTextWidth = 1f;
        private const int MaxBlockNameLength = 50;
        private const bool DiagnosticsEnabled = true;

        private List<Block> connectedBlocks = new List<Block>();
        private readonly UitkButton measureButton = new UitkButton();
        private readonly IBlockRectProvider rectProvider;

        public ConnectionGatherer(IBlockRectProvider rectProvider)
        {
            this.rectProvider = rectProvider;
            ApplyMeasureStyles(measureButton);
        }

        private static void ApplyMeasureStyles(UitkButton button)
        {
            if (button == null)
            {
                return;
            }

            var config = FlowchartWindow.Config;
            if (config != null)
            {
                if (config.BlockStyleSheet != null)
                {
                    button.styleSheets.Add(config.BlockStyleSheet);
                }

                if (config.SelectedBlockStyleSheet != null)
                {
                    button.styleSheets.Add(config.SelectedBlockStyleSheet);
                }
            }

            button.style.unityFontStyleAndWeight = FontStyle.Normal;
            button.style.fontSize = BaseFontSize;

            button.AddToClassList(DefaultBlockDrawer.BaseClass);
            button.AddToClassList(DefaultBlockDrawer.SelectedClass);
            button.EnableInClassList(DefaultBlockDrawer.SelectedClass, false);
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
                Rect fromRect = CalculateWindowRect(blockEl, drawCtx, fc);

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

                        Rect toRect = CalculateWindowRect(dest, drawCtx, fc);
                        if (OverlapsViewport(fromRect, toRect, viewRect))
                        {
                            result.Add(new ConnectionInfo(fromRect, toRect, shouldHighlight));
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

        private Rect CalculateWindowRect(Block block, DrawBlockContext drawCtx, Flowchart fc)
        {
            if (rectProvider != null && rectProvider.TryGetBlockRect(block, out Rect rect))
            {
                return rect;
            }

            Rect modelRect = block._NodeRect;

            string blockName = SafeBlockName(block);
            measureButton.text = blockName;

            float totalPaddingX = PaddingX * 2f;
            Vector2 unrestrictedSize = measureButton.MeasureTextSize(
                blockName,
                float.PositiveInfinity,
                MeasureMode.Undefined,
                float.PositiveInfinity,
                MeasureMode.Undefined);

            float baseTextWidth = SanitizeSize(unrestrictedSize.x, drawCtx.BlockMinWidth);
            float unclampedWidth = Mathf.Clamp(baseTextWidth + totalPaddingX, drawCtx.BlockMinWidth, drawCtx.BlockMaxWidth);
            float textWidthConstraint = Mathf.Max(unclampedWidth - totalPaddingX, MinTextWidth);

            Vector2 wrappedSize = measureButton.MeasureTextSize(
                blockName,
                textWidthConstraint,
                MeasureMode.AtMost,
                float.PositiveInfinity,
                MeasureMode.Undefined);

            float wrappedHeight = SanitizeSize(wrappedSize.y, drawCtx.DefaultBlockHeight);
            float height = Mathf.Max(drawCtx.DefaultBlockHeight, wrappedHeight + PaddingY);

            float zoom = 1f;
            Vector2 scrollPos = Vector2.zero;
            if (fc != null)
            {
                zoom = Mathf.Approximately(fc.Zoom, 0f) ? 1f : fc.Zoom;
                scrollPos = fc.ScrollPos;
            }

            modelRect.width = unclampedWidth * zoom;
            modelRect.height = height * zoom;

            if (drawCtx.UseGridSnap)
            {
                modelRect = modelRect.SnapPosition(drawCtx.GridObjectSnap);
            }

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
                Debug.Log($"[ConnectionGathererUitk] Bound={bound} does not overlap View={view}");
            }

            return bound.Overlaps(view);
        }

        private static string SafeBlockName(Block block)
        {
            string result = "New Block";
            if (block != null)
            {
                result = block.BlockName;
                if (result.Length > MaxBlockNameLength)
                {
                    result = result.Substring(0, MaxBlockNameLength);
                }
            }

            return result;
        }

        private static float SanitizeSize(float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                return fallback;
            }

            return value;
        }

        public void Dispose()
        {
            connectedBlocks.Clear();
        }
    }
}