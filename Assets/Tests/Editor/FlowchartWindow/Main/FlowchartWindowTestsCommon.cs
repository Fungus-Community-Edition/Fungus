using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    public class FlowchartWindowTestsCommon 
    {
        [SetUp]
        public virtual void SetUp()
        {
            TestUtils.ResetFlowchartWindowSingleton();

            // Create a Flowchart with three blocks at known positions
            PrepSceneObjects();
            void PrepSceneObjects()
            {
                host = new FakeFlowchartHost();
                host.Init();

                flowchart = host.Flowchart;

                blocks = new List<Block>();
                foreach (var pos in initBlockPositions)
                {
                    Block newBlock = host.CreateBlock(host.Flowchart, Vector2.zero);
                    newBlock.BlockName = $"Block @ {pos}";
                    newBlock._NodeRect = new Rect(pos, nodeSize);
                    blocks.Add(newBlock);
                }

                flowchart.ClearSelectedBlocks();

            }

            // Shared context
            ctx = new FlowchartContext
            {
                Flowchart = flowchart,
                Position = initCtxPos,
                FcHost = host,
            };

            // Common event templates
            mouseDown = new Event { type = EventType.MouseDown, button = MouseButton.Left  };
            mouseDrag = new Event { type = EventType.MouseDrag, button = MouseButton.Left };
            mouseReleased = new Event { type = EventType.MouseUp, button = MouseButton.Left };
        }

        protected FakeFlowchartHost host;
        protected Flowchart flowchart;
        protected IList<Block> blocks;
        protected static readonly IList<Vector2> initBlockPositions = new[] // In window space
        {
            new Vector2(10, 10),
            new Vector2(50, 50),
            new Vector2(100,100)
        };
        static readonly Vector2 nodeSize = new Vector2(20, 20);

        protected FlowchartContext ctx;
        protected static readonly Rect initCtxPos = new Rect(0, 0, 200, 200);
        protected Event mouseDown, mouseDrag, mouseReleased;

        [TearDown]
        public virtual void TearDown()
        {
            host.Dispose();
            host = null;
            flowchart = null;
            blocks = null;
            ctx = null;
            mouseDown = mouseDrag = mouseReleased = null;
        }
    }
}