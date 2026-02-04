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
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
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
            if (interaction == null || !interaction.SelectionBoxDragOngoing || !interaction.HasSelectionBox)
            {
                Debug.Log("No selection box to render.");
                return;
            }

            Rect selectionBox = interaction.SelectionBox;

            Painter2D painter = mgc.painter2D;
            painter.lineWidth = OutlineWidth;
            painter.strokeColor = OutlineColor;
            painter.fillColor = FillColor;

            painter.BeginPath();
            painter.MoveTo(new Vector2(selectionBox.xMin, selectionBox.yMin));
            painter.LineTo(new Vector2(selectionBox.xMax, selectionBox.yMin));
            painter.LineTo(new Vector2(selectionBox.xMax, selectionBox.yMax));
            painter.LineTo(new Vector2(selectionBox.xMin, selectionBox.yMax));
            painter.ClosePath();
            painter.Fill();
            painter.Stroke();
        }

        private const float OutlineWidth = 1f;
        private static readonly Color OutlineColor = new Color(0.27f, 0.54f, 0.93f, 0.9f);
        private static readonly Color FillColor = new Color(0.27f, 0.54f, 0.93f, 0.15f);
    }
}