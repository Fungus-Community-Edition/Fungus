using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils.FcWindow
{
    /// <summary>
    /// UITK-based connection renderer that draws using Painter2D.
    /// </summary>
    public sealed class ConnectionRenderer : VisualElement, IFlowchartWindowModule, IDisposable,
        IFlowchartChangeResponder, IWindowPanResponder, IScrollWheelMoveResponder,
        ILeftMouseDragStartResponder, ILeftMouseDragResponder, ILeftMouseDragEndResponder,
        IBlockSelectionResponder, IBlockDeselectionResponder, IMultiBlockSelectionResponder,
        IMultiBlockDeselectionResponder, IPreBlockDeletionResponder, IPostBlockDeletionResponder,
        IPostMultiBlockDeletionResponder, IBlockCreatedResponder, IBlocksCopiedResponder,
        ICommandSelectionResponder
    {
        public int Priority { get; set; } = 0;
        private const float DefaultBlockHeight = 40f;
        private const float BlockMinWidth = 60f;
        private const float BlockMaxWidth = 260f;

        private readonly FlowchartContext flowchartContext;
        private readonly DrawBlockContext drawBlockContext = new DrawBlockContext();
        private readonly ConnectionDrawer connectionDrawer;

        private FlowchartWindow owner;
        private bool isDisposed;

        public ConnectionRenderer(FlowchartContext context, ConnectionDrawer connectionDrawer)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            this.connectionDrawer = connectionDrawer ?? throw new ArgumentNullException(nameof(connectionDrawer));

            pickingMode = PickingMode.Ignore;
            this.contentContainer.StretchToParentSize();
            style.position = Position.Absolute;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.left = 0f;
            style.flexGrow = 1f;
        }

        public void Initialize(FlowchartWindow window)
        {
            owner = window ?? throw new ArgumentNullException(nameof(window));
            ToggleSubs(true);
        }

        void ToggleSubs(bool on)
        {
            if (on)
            {
                Undo.undoRedoPerformed += OnUndoRedoPerformed;
                generateVisualContent += OnGenerateVisualContent;
                RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
                RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
            else
            {
                Undo.undoRedoPerformed -= OnUndoRedoPerformed;
                generateVisualContent -= OnGenerateVisualContent;
                UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
                UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            }
        }

        private void OnUndoRedoPerformed()
        {
            RequestRepaint();
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            ToggleSubs(false);
            isDisposed = true;

            
            generateVisualContent -= OnGenerateVisualContent;

            connectionDrawer.Dispose();
            drawBlockContext.Dispose();
            RemoveFromHierarchy();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            RequestRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RequestRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (isDisposed)
            {
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                return;
            }

            UpdateDrawContext();
            connectionDrawer.Draw(mgc.painter2D, drawBlockContext, flowchartContext);
        }

        private void UpdateDrawContext()
        {
            if (owner != null)
            {
                flowchartContext.Position = owner.position;
            }

            drawBlockContext.FlowchartCtx = flowchartContext;
            drawBlockContext.DefaultBlockHeight = DefaultBlockHeight;
            drawBlockContext.BlockMinWidth = BlockMinWidth;
            drawBlockContext.BlockMaxWidth = BlockMaxWidth;

            float zoom = 1f;
            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart != null)
            {
                zoom = Mathf.Approximately(flowchart.Zoom, 0f) ? 1f : flowchart.Zoom;
            }

            Rect localRect = contentRect;
            drawBlockContext.ViewRect = new Rect(0f, 0f, localRect.width / zoom, localRect.height / zoom);
        }

        private void RequestRepaint()
        {
            if (isDisposed)
            {
                return;
            }

            MarkDirtyRepaint();
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next) => RequestRepaint();
        public void OnWindowPanned() => RequestRepaint();
        public void OnScrollWheelMoved() => RequestRepaint();
        public void OnLeftMouseDragStarted(PointerEventInfo info, Event evt) => RequestRepaint();
        public void OnLeftMouseDragged(PointerEventInfo info, Event evt) => RequestRepaint();
        public void OnLeftMouseDragEnded(PointerEventInfo info, Event evt) => RequestRepaint();
        public void OnBlockSelected(Block block) => RequestRepaint();
        public void OnBlockDeselected(Block block) => RequestRepaint();
        public void OnMultiBlocksSelected(IList<Block> blocks) => RequestRepaint();
        public void OnMultiBlocksDeselected(IList<Block> blocks) => RequestRepaint();
        public void OnPreBlockDeletion(IList<Block> blocks) => RequestRepaint();
        public void OnPreBlockDeletion(Block block) => RequestRepaint();
        public void OnPostBlockDeletion(ushort blockId) => RequestRepaint();
        public void OnPostMultiBlockDeletion(IList<ushort> blockIds) => RequestRepaint();
        public void OnBlockCreated(Block block) => RequestRepaint();
        public void OnBlocksCopied(IList<Block> blocks) => RequestRepaint();
        public void OnCommandSelected(Command command) => RequestRepaint();
    }
}