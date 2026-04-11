using AtMycelia.Graphics;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// Renders the current selection box stored in the flowchart interaction state.
    /// </summary>
    public sealed class SelectionBoxRenderer : VisualElement, IFlowchartWindowModule,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IScrollWheelMoveResponder, IWindowPanResponder, IFlowchartChangeResponder,
        IVisualResetter
    {
        public int Priority { get; set; } = 0;
        public SelectionBoxRenderer(FlowchartContext context)
        {
            fcContext = context ?? throw new ArgumentNullException(nameof(context));

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.flexGrow = 1f;
            this.SetPadding(0f);
            this.SetMargin(0f);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            generateVisualContent += OnGenerateVisualContent;
        }

        private readonly FlowchartContext fcContext;
        private bool isDisposed;

        public void Initialize(FlowchartWindow window)
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

        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt)
        {
            _shouldRender = !BlockHitTester.IsMouseOverBlock(info.FlowchartPosition);
            if (!_shouldRender)
            {
                //Debug.Log($"Selection box renderer: Not starting drag because mouse is over a block at {info.FlowchartPosition}");
            }
            RequestRepaint();
        }

        private bool _shouldRender;

        public void OnLeftMouseDragged(PointerEventInfo info, Event evt)
        {
            if (fcContext.Interaction.SelectionBox == Rect.zero)
            {
                return;
            }
            //Debug.Log($"Selection box renderer: Dragging with delta {info.FlowchartDelta}");
            RequestRepaint();
        }

        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt)
        {
            _ignoreSelectionBoxThisFrame = true;
            RequestRepaint();
            _shouldRender = false;
        }

        private bool _ignoreSelectionBoxThisFrame;
        // Resetting the selection box on drag end keeps blocks from being selected,
        // so we need to ignore rendering it for one frame instead when the dragging ends.

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
            if (isDisposed || !_shouldRender)
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

            var interaction = fcContext.Interaction;
            bool thereIsBoxToRender = interaction != null && interaction.SelectionBoxDragOngoing && 
                interaction.HasSelectionBox;
            if (!thereIsBoxToRender)
            {
                //Debug.Log("No selection box to render.");
                return;
            }

            Rect selectionBox = _ignoreSelectionBoxThisFrame ?
                                Rect.zero : 
                                interaction.SelectionBox;
            Painter2D painter = PrepPainter(mgc);
            DrawTheBox(painter, selectionBox);
            _ignoreSelectionBoxThisFrame = false;
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

        public void ResetVisuals()
        {
            if (isDisposed)
            {
                return;
            }

            _ignoreSelectionBoxThisFrame = false;
            _shouldRender = false;
            MarkDirtyRepaint();
        }
    }
}