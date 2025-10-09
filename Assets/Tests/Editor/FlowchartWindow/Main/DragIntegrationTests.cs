using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.VScripting.EditorUtils;
using Amanita.VScripting;

namespace VScriptingTests.FCWindowOperations.Integration
{
    public class DragIntegrationTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            pipeline = new FlowchartWindowInputHandler(
                new HitDetectionHandler(),
                new SingleSelectionHandler(),
                new BoxSelectionHandler(),
                new BlockDragHandler()
            );

            mouseButtonReleased = new Event { type = EventType.MouseUp, button = MouseButton.Left };
            mouseDrag = new Event { type = EventType.MouseDrag, button = MouseButton.Left, delta = dragDelta };
        }

        protected Event mouseButtonReleased;
        protected Vector2 dragDelta = new Vector2(5, 7);

        /// <summary>
        /// Simulates the "pre-pass" hit test done on MouseDown only.
        /// </summary>
        void PrePassHitTest(Event e)
        {
            if (e.type == EventType.MouseDown)
            {
                ctx.BlockHitInLastMouseDown = ctx.TopmostBlockOverlapping(e.mousePosition);
                // reset marquee on down
                ctx.SelectionBox = Rect.zero;
                ctx.SelectionBoxDragOngoing = false;
                ctx.BlockDragOngoing = false;
            }
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void Drag_SelectedBlock_MovesByDelta(int blockIndex)
        {
            // 1) MouseDown on block #1 to select + start drag
            Vector2 baseBlockPos = initBlockPositions[blockIndex];
            Block targetBlock = blocks[blockIndex];
            mouseDown.mousePosition = initBlockPositions[blockIndex];
            PrePassHitTest(mouseDown);

            bool downConsumed = pipeline.Process(mouseDown, ctx);
            Assert.IsTrue(downConsumed, "Should consume MouseDown on selected block");

            // block should now be selected and drag able to begin
            
            bool blockWasSelected = flowchart.SelectedBlocks.Contains(targetBlock);
            Assert.IsTrue(blockWasSelected, "Intended block was not selected");
            Assert.AreEqual(targetBlock, ctx.BlockHitInLastMouseDown,
                "Intended block wasn't the last one hit in mouse down");

            // 2) MouseDrag moves the block
            DragTheBlock();
            void DragTheBlock()
            {
                mouseDrag.mousePosition = baseBlockPos; // drag from that same initial pos
                PrePassHitTest(mouseDrag);             // no-op for drag
                bool dragConsumed = pipeline.Process(mouseDrag, ctx);
                Assert.IsTrue(dragConsumed, "Drag event should be consumed");
            }

            // Expected movement = delta / zoom (zoom=1)
            Vector2 expected = baseBlockPos + dragDelta;
            Assert.AreEqual(expected, targetBlock._NodeRect.position,
                "Block did not move by the correct delta");
            
            // 3) MouseUp finalizes & clears drag
            mouseButtonReleased.mousePosition = expected;
            PrePassHitTest(mouseButtonReleased);
            bool upConsumed = pipeline.Process(mouseButtonReleased, ctx);
            Assert.IsTrue(upConsumed, "MouseUp should be consumed to end drag");



            // After up, no BlockDragOngoing and DragBlock == null
            Assert.IsFalse(ctx.BlockDragOngoing, "DragOngoing should be cleared");
        }

        static IEnumerable<int> BlockIndices()
        {
            return Enumerable.Range(0, initBlockPositions.Count);
        }

        [Test]
        public void Drag_UnselectedBlock_DoesNothing()
        {
            // 1) MouseDown on block #0 but do NOT select it first
            Block targetBlock = blocks[0];
            Vector2 initBlockPos = initBlockPositions[0];
            mouseDown.mousePosition = initBlockPos;
            PrePassHitTest(mouseDown);

            bool downConsumed = pipeline.Process(mouseDown, ctx);

            // SingleSelectionHandler will clear+re-add, so it will select it
            // But BoxSelectionHandler ignores it, then BlockDragHandler should register it as
            // draggable
            Assert.IsTrue(downConsumed, "MouseDown should be consumed for selection");

            // Deselect for this test
            flowchart.ClearSelectedBlocks();
            ctx.BlockHitInLastMouseDown = targetBlock;
            ctx.RootBlockToDrag = null;

            // Now mouseDrag: no block selected so no drag
            mouseDrag.mousePosition = initBlockPos;
            PrePassHitTest(mouseDrag);
            bool dragConsumed = pipeline.Process(mouseDrag, ctx);
            Assert.IsFalse(dragConsumed, "Should not consume drag on unselected block");

            // block stays in place
            bool blockStayedInPlace = initBlockPos.Equals(targetBlock._NodeRect.position);
            Assert.IsTrue(blockStayedInPlace, $"Block did not stay in place");
        }

        [Test]
        public void DragOutsideEmpty_DoesNotStartBoxOrDrag()
        {
            // click in empty space
            mouseDown.mousePosition = new Vector2(150, 150);
            PrePassHitTest(mouseDown);
            bool downConsumed = pipeline.Process(mouseDown, ctx);
            Assert.IsTrue(downConsumed, "BoxSelectionHandler should consume down on empty");

            // drag in empty space: should continue marquee
            mouseDrag.mousePosition = new Vector2(160, 160);
            PrePassHitTest(mouseDrag);
            bool dragConsumed = pipeline.Process(mouseDrag, ctx);
            Assert.IsTrue(dragConsumed, "BoxSelectionHandler should consume drag");

            // mouse up: finalize marquee (select none)
            mouseButtonReleased.mousePosition = new Vector2(160, 160);
            PrePassHitTest(mouseButtonReleased);
            bool upConsumed = pipeline.Process(mouseButtonReleased, ctx);
            Assert.IsTrue(upConsumed, "BoxSelectionHandler should consume up");

            // no block selected
            Assert.IsEmpty(flowchart.SelectedBlocks);
        }
    }
}