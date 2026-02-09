using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// UITK-based connection renderer that draws using Painter2D.
    /// </summary>
    public sealed class ConnectionRendererUitk : VisualElement, IFlowchartWindowModule, IDisposable,
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
        private readonly ConnectionDrawerUitk connectionDrawer;

        private FlowchartWindowUitk owner;
        private bool isDisposed;

        public ConnectionRendererUitk(FlowchartContext context, ConnectionDrawerUitk connectionDrawer)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            this.connectionDrawer = connectionDrawer ?? throw new ArgumentNullException(nameof(connectionDrawer));

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.left = 0f;
            style.flexGrow = 1f;

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            generateVisualContent += OnGenerateVisualContent;
        }

        public void Initialize(FlowchartWindowUitk window)
        {
            owner = window ?? throw new ArgumentNullException(nameof(window));
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;

            UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
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
        public void OnPostBlockDeletion(uint blockId) => RequestRepaint();
        public void OnPostMultiBlockDeletion(IList<uint> blockIds) => RequestRepaint();
        public void OnBlockCreated(Block block) => RequestRepaint();
        public void OnBlocksCopied(IList<Block> blocks) => RequestRepaint();
        public void OnCommandSelected(Command command) => RequestRepaint();
    }
}