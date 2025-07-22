using Amanita.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.EditorUtils
{
    public class FlowchartContext
    {
        public IList<Block> SelectedBlocks
        {
            get { return Flowchart.SelectedBlocks; }
            set { Flowchart.SelectedBlocks = value; }
        }

        public virtual bool BlockDragOngoing { get; set; }
        public virtual bool SelectionBoxDragOngoing { get; set; }
        public virtual bool DragUndoRecorded { get; set; }
        public Vector2 StartDragPosition { get; set; }

        public virtual bool WeHitBlockInLastMouseDown => BlockHitInLastMouseDown != null;
        public virtual Block BlockHitInLastMouseDown { get; set; }

        public int ForceRepaintCount { get; set; }

        public virtual Vector2 StartSelectionBoxPosition { get; set; }
        
        public virtual bool HasDraggedSelected { get; set; }
        public virtual Block RootBlockToDrag { get; set; }

        // You'll want these set each frame right before the input processor does its thing
        public Flowchart Flowchart { get; set; }
        public virtual Rect Position { get; set; }
        public virtual Rect SelectionBox { get; set; } = default;
        public virtual IFlowchartHost FcHost { get; set; }
        public virtual IList<Block> AllBlocks { get; set; }

        /// <summary>
        /// Returns the topmost block whose NodeRect contains the given mouse position,
        /// taking scroll‐offset and zoom into account.
        /// </summary>
        public Block TopmostBlockOverlapping(Vector2 mousePosition)
        {
            Block result = null;
            var blocks = Flowchart.GetComponents<Block>();

            // Iterate in reverse order so higher‐z blocks get hit‐tested first
            for (int i = blocks.Length - 1; i >= 0; i--)
            {
                var currentBlock = blocks[i];
                // Transform the block’s _NodeRect into window-space
                Rect windowSpaceRect = currentBlock._NodeRect;
                windowSpaceRect.position += Flowchart.ScrollPos;

                var mousePosInWindowSpace = mousePosition / Flowchart.Zoom;

                if (windowSpaceRect.Contains(mousePosInWindowSpace))
                {
                    result = currentBlock;
                    break;
                }
            }

            return result;
        }

        public IList<Block> QueuedForDeletion
        {
            get { return queuedForDeletion; }
            set
            {
                queuedForDeletion.Clear();
                queuedForDeletion.AddRange(value);
            }
        }

        protected IList<Block> queuedForDeletion = new List<Block>();
        public virtual void SnapBlocksToGrid()
        {
            foreach (var elem in SelectedBlocks)
            {
                Undo.RecordObject(elem, "Block Position");
                elem._NodeRect = elem._NodeRect.SnapPosition(GridObjectSnap);
            }
            
        }

        public virtual float GridObjectSnap { get; set; } = 20;
    }
}