using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Amanita.EditorUtils.FlowchartWindow;

namespace Amanita.EditorUtils
{
    public class FlowchartContext
    {
        public Flowchart Flowchart { get; set; }
        public Vector2 VarScrollPos
        {
            get { return Flowchart.VariablesScrollPos; }
            set { Flowchart.VariablesScrollPos = value; }
        }

        public IList<Block> SelectedBlocks
        {
            get { return Flowchart.SelectedBlocks; }
            set { Flowchart.SelectedBlocks = value; }
        }

        public IList<Block> Blocks { get; set; }
        public Vector2 RightClickDown { get; set; }

        public bool DidDoubleClick { get; set; }
        public Vector2 StartDragPosition { get; set; }

        public int ForceRepaintCount { get; set; }

        public virtual IList<Block> MouseDownSelectionState { get; set; }
        public virtual Rect Position { get; set; }
        public virtual Vector2 StartSelectionBoxPosition { get; set; }
        public virtual Rect SelectionBox { get; set; }
        public virtual bool HasDraggedSelected { get; set; }
        public virtual Block DragBlock { get; set; }

        public virtual BlockInspector BlockInspector { get; set; }

        public virtual IList<BlockCopy> CopyList { get; set; }

        public virtual FlowchartWindow Window { get; set; }

        public IList<Block> HitTestables => Flowchart.GetComponents<Block>();

        /// <summary>
        /// Returns the topmost block whose NodeRect contains the given mouse position,
        /// taking scroll‐offset and zoom into account.
        /// </summary>
        public Block HitTest(Vector2 mousePosition)
        {
            Block result = null;
            var blocks = Flowchart.GetComponents<Block>();

            // Iterate in reverse order so higher‐z blocks get hit‐tested first
            for (int i = blocks.Length - 1; i >= 0; i--)
            {
                var currentBlock = blocks[i];
                // Transform the block’s _NodeRect into window-space
                Rect rect = currentBlock._NodeRect;
                rect.position += Flowchart.ScrollPos;

                var mousePosInFlowchartSpace = mousePosition / Flowchart.Zoom;

                if (rect.Contains(mousePosInFlowchartSpace))
                {
                    result = currentBlock;
                    break;
                }
            }

            return result;
        }


        public virtual void SnapBlocksToGrid()
        {
            foreach (var elem in SelectedBlocks)
            {
                Undo.RecordObject(elem, "Block Position");
                elem._NodeRect = elem._NodeRect.SnapPosition(FlowchartWindow.GridObjectSnap);
            }
            
        }
    }
}