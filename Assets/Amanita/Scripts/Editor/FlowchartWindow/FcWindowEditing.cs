using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    /// <summary>
    /// Handles user input (clicks, drags, shortcuts),
    /// block deletion, and clipboard operations.
    /// </summary>
    public class FcWindowEditing : IFcWindowComponent
    {
        private IFlowchartHost _window;
        private FlowchartWindowInputHandler _inputPipeline;
        private BlockClipboard _clipboard;

        // Blocks scheduled for deletion
        private readonly List<Block> _deleteList = new List<Block>();

        public void Initialize(FlowchartWindow window)
        {
            _window = window;

            // Build the input pipeline
            _inputPipeline = new FlowchartWindowInputHandler(
                new DeleteShortcutHandler(new FcWindowBlockDeletion(), KeyCode.Delete, new FcWindowFocusChecker()),
                new HitDetectionHandler(),
                new SingleSelectionHandler(),
                new BoxSelectionHandler(),
                new BlockDragHandler(),
                new PanZoomHandler(),
                new BlockContextMenuHandler(window, new GenericMenuFactory())
            );

            // Clipboard uses the same window host for copy/paste
            _clipboard = new BlockClipboard(window);
            window.Clipboard = _clipboard;
        }

        public void OnToolbarGUI()
        {
            // No toolbar UI here; toolbar belongs in its own component.
        }

        public void OnCanvasGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
            // 1) Process input events (selection, drag, pan/zoom, delete shortcut, etc.)
            if (_inputPipeline.Process(Event.current, fcCtx))
                Event.current.Use();

            // 2) Delete any blocks that were queued
            if (_deleteList.Count > 0)
            {
                DeleteScheduledBlocks();
                _window.Repaint();
            }
        }

        public void OnInspectorGUI()
        {
            // Nothing to draw in the inspector pane here
        }

        public void OnEditorUpdate()
        {
            // No per‐frame logic needed for editing right now
        }

        public virtual void QueueToDelete(IList<Block> toDelete)
        {
            foreach (var elem in toDelete)
            {
                QueueToDelete(elem);
            }
        }

        /// <summary>
        /// Public API for other components (or the window) to queue a block for deletion.
        /// </summary>
        public void QueueToDelete(Block block)
        {
            if (block != null && !_deleteList.Contains(block))
                _deleteList.Add(block);
        }

        /// <summary>
        /// Performs the actual destruction of queued blocks and their commands.
        /// </summary>
        private void DeleteScheduledBlocks()
        {
            foreach (var block in _deleteList)
            {
                // Destroy each command on the block
                foreach (var cmd in block.CommandList)
                    if (cmd != null)
                        Undo.DestroyObjectImmediate(cmd);

                // Destroy any event handler
                if (block._EventHandler != null)
                    Undo.DestroyObjectImmediate(block._EventHandler);

                // Deselect if needed
                if (block.IsSelected)
                    _window.Flowchart.DeselectBlockNoCheck(block);

                // Destroy the block itself
                Undo.DestroyObjectImmediate(block);
            }

            _deleteList.Clear();

            // Refresh block list and reselect the Flowchart root
            _window.UpdateBlockCollection();
            Selection.activeGameObject = _window.Flowchart.gameObject;
            _window.Flowchart.ClearSelectedCommands();
        }

        public void OnInspectorUpdate()
        {
            
        }
    }
}