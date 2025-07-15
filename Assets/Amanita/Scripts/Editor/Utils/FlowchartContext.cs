using System.Collections.Generic;
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
    }
}