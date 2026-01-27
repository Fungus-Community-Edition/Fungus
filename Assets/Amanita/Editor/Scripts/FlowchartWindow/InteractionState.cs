using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    /// <summary>
    /// Captures transient interaction state such as hit-testing,
    /// drag bookkeeping, and marquee selection metrics.
    /// </summary>
    public class InteractionState
    {
        public bool BlockDragOngoing { get; set; }
        public bool SelectionBoxDragOngoing { get; set; }
        public bool DragUndoRecorded { get; set; }
        public bool HasDraggedSelected { get; set; }

        public Vector2 StartDragPosition { get; set; }
        public Vector2 StartSelectionBoxPosition { get; set; }

        public Rect SelectionBox { get; set; } = Rect.zero;

        public Block BlockHitInLastMouseDown { get; set; }
        public Block RootBlockToDrag { get; set; }

        public bool WeHitBlockInLastMouseDown => BlockHitInLastMouseDown != null;

        public bool HasSelectionBox => SelectionBox.size != Vector2.zero;

        public void ResetSelectionBox()
        {
            SelectionBox = Rect.zero;
            StartSelectionBoxPosition = Vector2.zero;
            SelectionBoxDragOngoing = false;
        }

        public void ResetDragState()
        {
            BlockDragOngoing = false;
            DragUndoRecorded = false;
            HasDraggedSelected = false;
            RootBlockToDrag = null;
        }

        public void ResetHitState()
        {
            BlockHitInLastMouseDown = null;
        }
    }
}