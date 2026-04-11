using System;
using System.Collections.Generic;
using System.Linq;
using AtMycelia.Graphics;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// UITK-based grid renderer that redraws only when flowchart context changes,
    /// using FlowchartWindowSignals instead of the per-frame IMGUI loop.
    /// </summary>
    public sealed class GridRenderer : VisualElement, IFlowchartWindowModule,  IDisposable,
        IScrollWheelMoveResponder, IWindowPanResponder, IBlockSelectionResponder,
        IFlowchartChangeResponder, IVisualResetter
    {
        public int Priority { get; set; } = 0;
        private readonly FlowchartContext flowchartContext;
        private readonly DrawGridContext drawGridContext;
        private Vector2 cachedScrollPosition = new Vector2(float.NaN, float.NaN);
        private float cachedZoom = float.NaN;
        private Rect cachedContentRect = Rect.zero;
        private Block lastSelectedBlock;
        private bool isDisposed;

        private static readonly float SpacingScaleAtMinZoom = 0.5f;
        private const float DefaultZoomLevel = 1f;

        public GridRenderer(FlowchartContext context, DrawGridContext gridContext)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            drawGridContext = gridContext ?? throw new ArgumentNullException(nameof(gridContext));

            pickingMode = PickingMode.Ignore;
            style.flexGrow = 1f;
            style.width = Length.Percent(100);
            style.height = Length.Percent(100);

            ToggleSubs(true);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
                RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
                RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                generateVisualContent += OnGenerateVisualContent;
            }
            else
            {
                UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
                UnregisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
                UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                generateVisualContent -= OnGenerateVisualContent;
            }
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            QueueContextAwareRepaint(true);
        }

        private void QueueContextAwareRepaint(bool force)
        {
            if (force)
            {
                MarkDirtyRepaint();
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                return;
            }

            bool scrollChanged = !Mathf.Approximately(flowchart.ScrollPos.x, cachedScrollPosition.x)
                || !Mathf.Approximately(flowchart.ScrollPos.y, cachedScrollPosition.y);

            bool zoomChanged = !Mathf.Approximately(flowchart.Zoom, cachedZoom);

            bool sizeChanged = !Mathf.Approximately(contentRect.width, cachedContentRect.width)
                || !Mathf.Approximately(contentRect.height, cachedContentRect.height);

            if (scrollChanged || zoomChanged || sizeChanged)
            {
                MarkDirtyRepaint();
            }
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            // No action needed on detach for now.
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            ToggleSubs(false);
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            // Even when there's no Flowchart, we still want to generate the grid.
            float zoom = 1f;
            if (flowchartContext.Flowchart != null)
            {
                zoom = Mathf.Approximately(flowchartContext.Flowchart.Zoom, 0f)
                    ? 1f
                    : flowchartContext.Flowchart.Zoom;
            }

            Vector2 scrollPos = flowchartContext.Flowchart != null
                ? flowchartContext.Flowchart.ScrollPos
                : Vector2.zero;

            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float spacing = CalculateAdaptiveSpacing(zoom);

            float viewWidth = rect.width / zoom;
            float viewHeight = rect.height / zoom;

            IList<float> verticalLines = GridUtils.GetVerticalLinePositions(
                scrollPos.x,
                viewWidth,
                spacing);

            IList<float> horizontalLines = GridUtils.GetHorizontalLinePositions(
                scrollPos.y,
                viewHeight,
                spacing);

            Painter2D painter = mgc.painter2D;
            painter.lineWidth = 1f;
            painter.strokeColor = drawGridContext.GridLineColor;
            painter.fillColor = Color.clear;

            DrawVerticalLines(painter, verticalLines, rect.height, zoom);
            DrawHorizontalLines(painter, horizontalLines, rect.width, zoom);

            cachedScrollPosition = scrollPos;
            cachedZoom = zoom;
            cachedContentRect = rect;
        }

        private static void DrawVerticalLines(Painter2D painter, IList<float> xPositions, float viewHeight, float zoom)
        {
            for (int i = 0; i < xPositions.Count; i++)
            {
                float x = xPositions[i] * zoom;
                painter.BeginPath();
                painter.MoveTo(new Vector2(x, 0f));
                painter.LineTo(new Vector2(x, viewHeight));
                painter.Stroke();
            }
        }

        private static void DrawHorizontalLines(Painter2D painter, IList<float> yPositions, float viewWidth, float zoom)
        {
            for (int i = 0; i < yPositions.Count; i++)
            {
                float y = yPositions[i] * zoom;
                painter.BeginPath();
                painter.MoveTo(new Vector2(0f, y));
                painter.LineTo(new Vector2(viewWidth, y));
                painter.Stroke();
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            bool widthChanged = !Mathf.Approximately(evt.newRect.width, evt.oldRect.width);
            bool heightChanged = !Mathf.Approximately(evt.newRect.height, evt.oldRect.height);

            if (widthChanged || heightChanged)
            {
                cachedContentRect = evt.newRect;
                QueueContextAwareRepaint(true);
            }
        }

        public void RefreshNow()
        {
            QueueContextAwareRepaint(true);
        }

        public void OnScrollWheelMoved()
        {
            QueueContextAwareRepaint(true);
        }

        public void OnWindowPanned()
        {
            QueueContextAwareRepaint(true);
        }

        public void OnBlockSelected(Block block)
        {
            if (ReferenceEquals(block, lastSelectedBlock))
            {
                return;
            }

            lastSelectedBlock = block;
            lastBlocksSelected.Clear();
            lastBlocksSelected.Add(block);
            QueueContextAwareRepaint(false);
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            lastSelectedBlock = next != null ? next.SelectedBlock : null;
            cachedScrollPosition = new Vector2(float.NaN, float.NaN);
            cachedZoom = float.NaN;
            QueueContextAwareRepaint(true);
        }

        public void Initialize(FlowchartWindow window)
        {
            
        }

        private float CalculateAdaptiveSpacing(float currentZoom)
        {
            float baseSpacing = Mathf.Approximately(drawGridContext.GridLineSpacingSize, 0f)
                ? 1f
                : drawGridContext.GridLineSpacingSize;

            float minZoom = FlowchartWindow.Config.MinZoom;
            float normalized = Mathf.Clamp01(Mathf.InverseLerp(minZoom, DefaultZoomLevel, currentZoom));
            float spacingMultiplier = Mathf.Lerp(SpacingScaleAtMinZoom, 1f, normalized);

            return baseSpacing * spacingMultiplier;
        }

        public void OnMultiBlocksSelected(IList<Block> blocks)
        {
            lastSelectedBlock = null; // Since that var is for when just a single one is selected.
            bool alreadySelectedThese = blocks.SequenceEqual(lastBlocksSelected);
            if (alreadySelectedThese)
            {
                return;
            }

            lastBlocksSelected.Clear();
            foreach (Block block in blocks)
            {
                lastBlocksSelected.Add(block);
            }
        }

        private readonly IList<Block> lastBlocksSelected = new List<Block>();

        public void ResetVisuals()
        {
            if (isDisposed)
            {
                return;
            }

            cachedScrollPosition = new Vector2(float.NaN, float.NaN);
            cachedZoom = float.NaN;
            cachedContentRect = Rect.zero;
            QueueContextAwareRepaint(true);
        }
    }
}