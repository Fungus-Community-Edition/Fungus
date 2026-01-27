using Collections;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartContext : IDisposable
    {
        public FlowchartDocument Document { get; } = new FlowchartDocument();
        public SelectionState Selection { get; } = new SelectionState();
        public InteractionState Interaction { get; } = new InteractionState();

        public virtual void Dispose()
        {
            SelectionBoxDragOngoing = DragUndoRecorded = false;
            StartDragPosition = StartSelectionBoxPosition = default;
            ForceRepaintCount = 0;
            BlockHitInLastMouseDown = RootBlockToDrag = null;
            Position = SelectionBox = default;
            QueuedForDeletion.Clear();
            Flowchart = null;
        }

        public virtual bool SelectionBoxDragOngoing
        {
            get { return Interaction.SelectionBoxDragOngoing; }
            set { Interaction.SelectionBoxDragOngoing = value; }
        }

        public virtual bool DragUndoRecorded
        {
            get { return Interaction.DragUndoRecorded; }
            set { Interaction.DragUndoRecorded = value; }
        }

        public Vector2 StartDragPosition
        {
            get { return Interaction.StartDragPosition; }
            set { Interaction.StartDragPosition = value; }
        }

        public virtual bool WeHitBlockInLastMouseDown => Interaction.WeHitBlockInLastMouseDown;

        public virtual Block BlockHitInLastMouseDown
        {
            get { return Interaction.BlockHitInLastMouseDown; }
            set { Interaction.BlockHitInLastMouseDown = value; }
        }

        public int ForceRepaintCount { get; set; }

        public virtual Vector2 StartSelectionBoxPosition
        {
            get { return Interaction.StartSelectionBoxPosition; }
            set { Interaction.StartSelectionBoxPosition = value; }
        }

        public virtual bool HasDraggedSelected
        {
            get { return Interaction.HasDraggedSelected; }
            set { Interaction.HasDraggedSelected = value; }
        }

        public virtual Block RootBlockToDrag
        {
            get { return Interaction.RootBlockToDrag; }
            set { Interaction.RootBlockToDrag = value; }
        }

        private Flowchart flowchart;

        public Flowchart Flowchart
        {
            get { return flowchart; }
            set
            {
                flowchart = value;
                Document.Flowchart = value;
                Selection.Flowchart = value;
            }
        }

        public virtual Rect Position { get; set; }

        public virtual Rect SelectionBox
        {
            get { return Interaction.SelectionBox; }
            set { Interaction.SelectionBox = value; }
        }

        public virtual IFlowchartHost FcHost { get; set; }

        public IList<Block> QueuedForDeletion
        {
            get { return queuedForDeletion; }
            set
            {
                queuedForDeletion.Clear();
                if (value == null)
                {
                    return;
                }

                foreach (var block in value)
                {
                    queuedForDeletion.Add(block);
                }
            }
        }

        protected IList<Block> queuedForDeletion = new List<Block>();

        public virtual void SnapBlocksToGrid()
        {
            foreach (var elem in Selection.Blocks)
            {
                Undo.RecordObject(elem, "Block Position");
                elem._NodeRect = elem._NodeRect.SnapPosition(GridObjectSnap);
            }
        }

        public virtual float GridObjectSnap { get; set; } = 20;
    }
}
