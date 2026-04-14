using System;
using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    /// <summary>
    /// Captures transient interaction state such as hit-testing,
    /// drag bookkeeping, and marquee selection metrics.
    /// </summary>
    public class InteractionState : IDisposable
    {
        public bool BlockDragOngoing { get; set; }
        public bool SelectionBoxDragOngoing { get; set; }
        public bool DragUndoRecorded { get; set; }
        public bool HasDraggedSelected { get; set; }

        public Vector2 StartDragPosition { get; set; }
        public Vector2 SelectionBoxStartPos { get; set; }

        public Rect SelectionBox { get; set; } = Rect.zero;

        public Block BlockHitInLastMouseDown { get; set; }
        public Block RootBlockToDrag { get; set; }

        public bool WeHitBlockInLastMouseDown => BlockHitInLastMouseDown != null;

        public bool HasSelectionBox => SelectionBox.size != Vector2.zero;

        public void ResetSelectionBox()
        {
            SelectionBox = Rect.zero;
            SelectionBoxStartPos = Vector2.zero;
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

        public virtual void Dispose()
        {
            ResetDragState();
            ResetSelectionBox();
            ResetHitState();
            RootBlockToDrag = null;
        }
    }
}