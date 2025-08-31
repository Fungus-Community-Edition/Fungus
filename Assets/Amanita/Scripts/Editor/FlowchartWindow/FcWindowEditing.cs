using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Handles user input (clicks, drags, shortcuts),
    /// block deletion, and clipboard operations.
    /// </summary>
    public class FcWindowEditing : IFcWindowComponent
    {
        public virtual void Initialize(IFlowchartHost window)
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

        protected IFlowchartHost _window;
        protected FlowchartWindowInputHandler _inputPipeline;
        protected BlockClipboard _clipboard;

        public virtual void OnToolbarGUI()
        {
            // No toolbar UI here; toolbar belongs in its own component.
        }

        public virtual void OnGUI(DrawBlockContext drawCtx, FlowchartContext fcCtx)
        {
            if (_inputPipeline.Process(Event.current, fcCtx))
                Event.current.Use();

            if (_scheduledForDeletion.Count > 0)
            {
                DeleteScheduledBlocks();
                _window.Repaint();
            }
        }

        public virtual void OnGUI()
        {
            if (_scheduledForDeletion.Count > 0)
            {
                DeleteScheduledBlocks();
                _window.Repaint();
            }
        }

        protected List<Block> _scheduledForDeletion = new List<Block>();

        public virtual void OnInspectorGUI()
        {
        }

        public virtual void OnEditorUpdate()
        {
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
        public virtual void QueueToDelete(Block block)
        {
            if (block != null && !_scheduledForDeletion.Contains(block))
                _scheduledForDeletion.Add(block);
        }

        /// <summary>
        /// Performs the actual destruction of queued blocks and their commands.
        /// </summary>
        protected virtual void DeleteScheduledBlocks()
        {
            foreach (var block in _scheduledForDeletion)
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
                FlowchartWindowSignals.PreBlockDeletion(block);
                Undo.DestroyObjectImmediate(block);
            }

            _scheduledForDeletion.Clear();

            // Refresh block list and reselect the Flowchart root
            _window.UpdateBlockCollection();
            Selection.activeGameObject = _window.Flowchart.gameObject;
            _window.Flowchart.ClearSelectedCommands();
        }

        public virtual void OnInspectorUpdate()
        {
            
        }

        public virtual void Dispose()
        {
            _window = null;
            _inputPipeline.Dispose();
            _inputPipeline = null;
            _clipboard = null; // We expect another module to dispose of the clipboard
            _scheduledForDeletion.Clear();
            _scheduledForDeletion = null;
        }
    }
}
