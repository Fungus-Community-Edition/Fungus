using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Amanita.Tests.Editor.Integration
{
    public class DragIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            // Create flowchart + 3 blocks
            PrepSceneObjects();
            void PrepSceneObjects()
            {
                fcHolder = new GameObject("fc");
                flowchart = fcHolder.AddComponent<Flowchart>();
                blocks = new List<Block>();

                foreach (var pos in initBlockPositions)
                {
                    var newBlock = flowchart.CreateBlock(Vector2.zero);
                    newBlock._NodeRect = new Rect(pos, nodeSize);
                    blocks.Add(newBlock);
                }
            }

            // Build pipeline: click select → box select → drag
            pipeline = new FlowchartWindowInputHandler(
                new SingleSelectionHandler(),
                new BoxSelectionHandler(),
                new BlockDragHandler()
            );

            // Shared context
            ctx = new FlowchartContext
            {
                Flowchart = flowchart,
                Position = initCtxPos
            };

            // Common events
            mouseDown = new Event { type = EventType.MouseDown, button = 0 };
            mouseDrag = new Event { type = EventType.MouseDrag, button = 0, delta = dragDelta };
            mouseButtonReleased = new Event { type = EventType.MouseUp, button = 0 };
        }

        GameObject fcHolder;
        Flowchart flowchart;
        IList<Block> blocks;
        static readonly IList<Vector2> initBlockPositions = new Vector2[]{
            new Vector2(10, 10),
            new Vector2(50, 50),
            new Vector2(100,100)
        };
        static readonly Vector2 nodeSize = new Vector2(20, 20);

        FlowchartWindowInputHandler pipeline;

        FlowchartContext ctx;
        static readonly Rect initCtxPos = new Rect(0, 0, 200, 200);

        Event mouseDown, mouseDrag, mouseButtonReleased;


        Vector2 dragDelta = new Vector2(5, 7);

        [TearDown]
        public void TearDown()
        {
            UnityObject.DestroyImmediate(fcHolder);
            ctx = null;
            mouseDown = mouseDrag = mouseButtonReleased = null;
        }

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