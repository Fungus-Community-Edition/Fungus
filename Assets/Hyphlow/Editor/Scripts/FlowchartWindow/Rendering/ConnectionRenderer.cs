using AtMycelia.Graphics;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
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
        ICommandSelectionResponder, IVisualResetter
    {
        public int Priority { get; set; } = 0;
        private const float DefaultBlockHeight = 40f;
        private const float BlockMinWidth = 60f;
        private const float BlockMaxWidth = 260f;

        private const bool DiagnosticsEnabled = true;
        private int diagnosticsRemaining = 6;

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
            style.position = Position.Absolute;
            style.flexGrow = 1f;
            this.contentContainer.StretchToParentSize();
        }

        public void Initialize(FlowchartWindow window)
        {
            owner = window != null ? 
                window : 
                throw new ArgumentNullException(nameof(window));
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
            LogDiagnostics("AttachToPanel");
            RequestRepaint();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            LogDiagnostics($"GeometryChanged newRect={evt.newRect}");
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
            LogDiagnostics($"GenerateVisualContent contentRect={contentRect} viewRect={drawBlockContext.ViewRect}");
            connectionDrawer.Draw(mgc.painter2D, drawBlockContext, flowchartContext);
        }

        private void LogDiagnostics(string message)
        {
            if (!DiagnosticsEnabled || diagnosticsRemaining <= 0)
            {
                return;
            }

            diagnosticsRemaining--;
            //Debug.Log($"[ConnectionRenderer] {message} frame={Time.frameCount}");
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

            Rect viewRectSource = contentRect;
            if (viewRectSource.width <= 0f || viewRectSource.height <= 0f)
            {
                viewRectSource = flowchartContext.Position;
            }

            drawBlockContext.ViewRect = new Rect(0f, 0f, viewRectSource.width, viewRectSource.height);
        }

        private void RequestRepaint()
        {
            if (isDisposed)
            {
                return;
            }

            MarkDirtyRepaint();
        }

        public void ResetVisuals()
        {
            RequestRepaint();
        }

        #region Just request a repaint for all of these events
        // Since any of them could change the connections that need to be drawn.
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
        #endregion
    }
}