using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Renders the current selection box stored in the flowchart interaction state.
    /// </summary>
    public sealed class SelectionBoxRendererUitk : VisualElement, IFlowchartWindowModule,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IScrollWheelMoveResponder, IWindowPanResponder, IFlowchartChangeResponder
    {
        public SelectionBoxRendererUitk(FlowchartContext context)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.flexGrow = 1f;

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            generateVisualContent += OnGenerateVisualContent;
        }

        private readonly FlowchartContext flowchartContext;
        private bool isDisposed;

        public void Initialize(FlowchartWindowUitk window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            isDisposed = false;
            BringToFront();
        }

        private void OnAttachedToPanel(AttachToPanelEvent _)
        {
            BringToFront();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            generateVisualContent -= OnGenerateVisualContent;
            RemoveFromHierarchy();
        }

        public void OnLeftMouseDragStarted(Vector2 startPos, Event evt)
        {
            RequestRepaint();
        }

        public void OnLeftMouseDragged(Vector2 delta, Event evt)
        {
            RequestRepaint();
        }

        public void OnLeftMouseDragEnded(Vector2 endPos, Event evt)
        {
            Debug.Log($"Selection box renderer: Drag ended at {endPos}");
            flowchartContext.Interaction?.ResetSelectionBox(); // In case it wasn't already reset
            RequestRepaint();
        }

        public void OnScrollWheelMoved()
        {
            RequestRepaint();
        }

        public void OnWindowPanned()
        {
            RequestRepaint();
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            RequestRepaint();
        }

        private void RequestRepaint()
        {
            if (isDisposed)
            {
                return;
            }

            MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (isDisposed)
            {
                return;
            }

            var interaction = flowchartContext.Interaction;
            bool thereIsBoxToRender = interaction != null && interaction.SelectionBoxDragOngoing && 
                interaction.HasSelectionBox;
            if (!thereIsBoxToRender)
            {
                Debug.Log("No selection box to render.");
                return;
            }

            Rect selectionBox = interaction.SelectionBox;
            Painter2D painter = PrepPainter(mgc);
            DrawTheBox(painter, selectionBox);
        }

        Painter2D PrepPainter(MeshGenerationContext mgc)
        {
            Painter2D painter = mgc.painter2D;
            painter.lineWidth = OutlineWidth;
            painter.strokeColor = OutlineColor;
            painter.fillColor = FillColor;
            return painter;
        }

        private const float OutlineWidth = 1f;
        private static readonly Color OutlineColor = new Color(0.27f, 0.54f, 0.93f, 0.9f);
        private static readonly Color FillColor = new Color(0.27f, 0.54f, 0.93f, 0.15f);

        void DrawTheBox(Painter2D painter, Rect selectionBox)
        {
            painter.BeginPath();
            painter.MoveTo(new Vector2(selectionBox.xMin, selectionBox.yMin));
            painter.LineTo(new Vector2(selectionBox.xMax, selectionBox.yMin));
            painter.LineTo(new Vector2(selectionBox.xMax, selectionBox.yMax));
            painter.LineTo(new Vector2(selectionBox.xMin, selectionBox.yMax));
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();
        }

        
    }
}