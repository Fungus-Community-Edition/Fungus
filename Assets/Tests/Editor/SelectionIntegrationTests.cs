using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityObject = UnityEngine.Object;

namespace Amanita.Tests.Editor.Integration
{
    public class SelectionIntegrationTests
    {
        [SetUp]
        public void SetUp()
        {
            // Create a Flowchart with three blocks at known positions
            PrepSceneObjects();
            void PrepSceneObjects()
            {
                fcHolder = new GameObject("Flowchart");
                flowchart = fcHolder.AddComponent<Flowchart>();
                blocks = new List<Block>();
                foreach (var pos in initBlockPositions)
                {
                    var newBlock = flowchart.CreateBlock(Vector2.zero);
                    newBlock._NodeRect = new Rect(pos, nodeSize);
                    blocks.Add(newBlock);
                }
            }

            // Build handlers pipeline: single click → box → (drag would follow)
            pipeline = new FlowchartWindowInputHandler
            (
                new SingleSelectionHandler(),
                new BoxSelectionHandler()
            );

            // Shared context
            ctx = new FlowchartContext
            {
                Flowchart = flowchart,
                Position = new Rect(0, 0, 200, 200),
                Window = null // not used by these handlers
            };

            // Common event templates
            mouseDown = new Event { type = EventType.MouseDown, button = leftMouseButton };
            mouseDrag = new Event { type = EventType.MouseDrag, button = leftMouseButton };
            mouseReleased = new Event { type = EventType.MouseUp, button = leftMouseButton };
        }

        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected IList<Block> blocks;
        static readonly IList<Vector2> initBlockPositions = new[] // In window space
        {
            new Vector2(10, 10),
            new Vector2(50, 50),
            new Vector2(100,100)
        };
        static readonly Vector2 nodeSize = new Vector2(20, 20);

        protected FlowchartWindowInputHandler pipeline;
        protected FlowchartContext ctx;
        protected Event mouseDown, mouseDrag, mouseReleased;
        protected static readonly int leftMouseButton = 0;


        [TearDown]
        public void TearDown()
        {
            UnityObject.DestroyImmediate(fcHolder);
            ctx = null;
            mouseDown = mouseDrag = mouseReleased = null;
        }

        /// <summary>
        /// Simulates the “pre-pass” hit test that FlowchartWindow.OnGUI does
        /// by setting BlockHitInLastMouseDown on the context.
        /// </summary>
        void PrePassHitTest(Event e)
        {
            if (e.type == EventType.MouseDown)
            {
                ctx.BlockHitInLastMouseDown = ctx.TopmostBlockOverlapping(e.mousePosition);
            }
            // clear any old marquee state
            if (e.type == EventType.MouseDown)
                ctx.SelectionBox = Rect.zero;
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void ClickOnBlock_SelectsThatBlock(int blockIndex)
        {
            // pick the 2nd block at position (50,50)
            mouseDown.mousePosition = initBlockPositions[1];
            PrePassHitTest(mouseDown);

            // run pipeline
            bool consumed = pipeline.Process(mouseDown, ctx);

            // SingleSelectionHandler never consumes, BoxSelectionHandler sees a hit->no consume
            Assert.IsFalse(consumed);

            // Expect exactly that block to be selected
            Block blockWeExpect = blocks[blockIndex];
            CollectionAssert.AreEqual(
                new[] { blocks[1] },
                flowchart.SelectedBlocks,
                "Click on block did not select exactly that block"
            );
        }

        static IEnumerable<int> BlockIndices()
        {
            return Enumerable.Range(0, initBlockPositions.Count);
        }

        [Test]
        public void ClickOnEmpty_ClearsSelection()
        {
            // Pre-populate a selection
            flowchart.AddSelectedBlock(blocks[0]);

            // Click at empty space (e.g. at (0,0))
            mouseDown.mousePosition = Vector2.zero;
            PrePassHitTest(mouseDown);

            bool consumed = pipeline.Process(mouseDown, ctx);
            Assert.IsTrue(consumed, "BoxSelectionHandler should consume MouseDown on empty space");

            // Expect no selected blocks
            Assert.IsEmpty(
                flowchart.SelectedBlocks,
                "Click on empty space should clear selection"
            );
        }

        [Test]
        public void Marquee_SelectsAllOverlappedBlocks()
        {
            // Drag box from (0,0) to (60,60) → should include blocks[0] and blocks[1]
            // 1) MouseDown at (0,0)
            mouseDown.mousePosition = Vector2.zero;
            PrePassHitTest(mouseDown);
            bool downConsumed = pipeline.Process(mouseDown, ctx);
            Assert.IsTrue(downConsumed, "BoxSelectionHandler should consume MouseDown on empty");

            // 2) MouseDrag to (60,60)
            mouseDrag.mousePosition = new Vector2(60, 60);
            PrePassHitTest(mouseDrag); // blockHitInLastMouseDown unchanged
            bool dragConsumed = pipeline.Process(mouseDrag, ctx);
            Assert.IsTrue(dragConsumed, "BoxSelectionHandler should consume MouseDrag");

            // 3) MouseUp at (60,60)
            mouseReleased.mousePosition = new Vector2(60, 60);
            PrePassHitTest(mouseReleased);
            bool upConsumed = pipeline.Process(mouseReleased, ctx);
            Assert.IsTrue(upConsumed, "BoxSelectionHandler should consume MouseUp");

            // Verify selection contains blocks 0 and 1 only
            var sel = flowchart.SelectedBlocks;
            CollectionAssert.AreEquivalent(
                new[] { blocks[0], blocks[1] },
                sel,
                "Marquee should select only blocks whose rects overlap the box"
            );
        }
    }
}