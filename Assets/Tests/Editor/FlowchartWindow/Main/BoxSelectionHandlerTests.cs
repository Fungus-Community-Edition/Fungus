using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow;

namespace VScriptingTests.FCWindowOperations
{
    public class BoxSelectionHandlerTests
    {
        FlowchartContext ctx;
        BoxSelectionHandler handler;
        Event down, drag, mouseRelease;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("TestFlowchart");
            flowchart = go.AddComponent<Flowchart>();
            // Create 1 block for each init node pos
            blocks = initNodePositions.Select(pos =>
            {
                var newBlock = flowchart.CreateBlock(Vector2.zero);
                newBlock._NodeRect = new Rect(pos, nodeSize);
                return newBlock;
            }).ToArray();

            ctx = new FlowchartContext
            {
                Flowchart = flowchart,
                Position = new Rect(0, 0, 200, 200)
            };
            handler = new BoxSelectionHandler();

            down = new Event { type = EventType.MouseDown, button = leftMouseButton };
            drag = new Event { type = EventType.MouseDrag, button = leftMouseButton };
            mouseRelease = new Event { type = EventType.MouseUp, button = leftMouseButton };
        }

        protected Flowchart flowchart;
        protected IList<Block> blocks;
        protected readonly Vector2[] initNodePositions =
        {
            new Vector2(10, 10),
            new Vector2(50, 50),
            new Vector2(90, 20)
        };
        protected readonly Vector2 nodeSize = new Vector2(20, 20);
        protected readonly int leftMouseButton = 0;

        [Test]
        public void MouseDown_OnEmptySpace_InitializesSelectionBox()
        {
            down.mousePosition = new Vector2(0, 0);
            bool consumed = handler.Handle(down, ctx);
            var interaction = ctx.Interaction;
            Assert.IsTrue(consumed);
            Assert.AreNotEqual(default, interaction.SelectionBox);
            Assert.AreEqual(down.mousePosition, interaction.SelectionBoxStartPos);
        }

        [Test]
        public void MouseDown_OnBlock_DoesNothing()
        {
            down.mousePosition = initNodePositions[1];
            var interaction = ctx.Interaction;
            interaction.BlockHitInLastMouseDown = blocks[0];
            Assert.IsFalse(handler.Handle(down, ctx), "Down event gets consumed by handler");

            bool thereIsNoSelectionBox = interaction.SelectionBox == default;
            Assert.IsTrue(thereIsNoSelectionBox, "Just mouse down creates a valid selection box. It shouldn't.");
        }

        [Test]
        public void MouseDrag_ExpandsSelectionBox()
        {
            Vector2 startPos = Vector2.zero;
            Vector2 endPos = new Vector2(30, 40);
            SimulateMakingSelectionBox(startPos, endPos);

            var rect = ctx.Interaction.SelectionBox;
            Assert.AreEqual(startPos, rect.position);
            // ^Since the position of a rect is meant to be equal to that of its bottom left corner by default

            Vector2 expectedSize = endPos;
            Assert.AreEqual(expectedSize, rect.size);
        }

        protected virtual void SimulateMakingSelectionBox(Vector2 firstPoint, Vector2 lastPoint)
        {
            down.mousePosition = firstPoint;
            handler.Handle(down, ctx);

            drag.mousePosition = lastPoint;
            handler.Handle(drag, ctx);
        }

        [Test]
        public void MouseRelease_SelectsAllOverlappedBlocks()
        {
            Vector2 firstPoint = Vector2.zero;
            Vector2 lastPoint = new Vector2(60, 60);

            SimulateMakingFullSelection(firstPoint, lastPoint);

            var selected = flowchart.SelectedBlocks;
            bool containsFirstBlock = selected.Contains(blocks[0]);
            Assert.IsTrue(containsFirstBlock);

            bool containsSecondBlock = selected.Contains(blocks[1]);
            Assert.IsTrue(containsSecondBlock);

            bool doesNOTContainThirdBlock = !selected.Contains(blocks[2]);
            Assert.IsTrue(doesNOTContainThirdBlock);
        }

        protected virtual void SimulateMakingFullSelection(Vector2 firstPoint, Vector2 lastPoint)
        {
            SimulateMakingSelectionBox(firstPoint, lastPoint);

            // Releasing the mouse should finalize the selection
            mouseRelease.mousePosition = lastPoint;
            handler.Handle(mouseRelease, ctx);
        }
    }
}