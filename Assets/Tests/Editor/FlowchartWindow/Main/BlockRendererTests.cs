using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Amanita.EditorUtils;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class BlockRendererTests
    {
        // Test doubles
        class FakeDrawer : IBlockDrawer
        {
            public List<(Block Block, BlockGraphics Graphics)> DrawCalls
                = new List<(Block, BlockGraphics)>();

            public void Draw(Block toDraw, DrawBlockContext drawCtx)
            {
                DrawCalls.Add((toDraw, drawCtx.Graphics));
            }
        }

        class FakeGraphicsGenerator : IBlockGraphicsGenerator
        {
            public List<Block> GeneratedFor = new List<Block>();
            public List<BlockGraphics> ReturnedGraphics = new List<BlockGraphics>();

            public BlockGraphics GenerateFor(Block block)
            {
                GeneratedFor.Add(block);
                // Return a distinct BlockGraphics for each call
                var g = new BlockGraphics();
                ReturnedGraphics.Add(g);
                return g;
            }
        }

        FlowchartContext _flowchartCtx;
        DrawBlockContext _drawCtx;
        FakeFlowchartHost _host;
        Block _insideBlock;
        Block _outsideBlock;
        FakeDrawer _drawer;
        FakeGraphicsGenerator _gfxGen;
        BlockRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            // 1) Create host + flowchart
            _host = new FakeFlowchartHost();
            _host.Init();
            var fc = _host.Flowchart;

            // 2) Create two blocks, one inside a 100×100 view, one outside
            _insideBlock = _host.CreateBlock(fc, Vector2.zero);
            _insideBlock._NodeRect = new Rect(10, 10, 20, 20);

            _outsideBlock = _host.CreateBlock(fc, Vector2.zero);
            _outsideBlock._NodeRect = new Rect(200, 200, 20, 20);

            // 3) Prepare FlowchartContext
            _flowchartCtx = new FlowchartContext
            {
                Flowchart = fc,
                Position = new Rect(0, 0, 100, 100),  // window size in screen‐space
                FcHost = _host,
                AllBlocks = new List<Block> { _insideBlock, _outsideBlock }
            };

            // 4) Prepare DrawBlockContext
            _drawCtx = new DrawBlockContext
            {
                FlowchartCtx = _flowchartCtx,
                BlockMinWidth = 60,
                BlockMaxWidth = 240,
                DefaultBlockHeight = 40,
                NodeStyle = new GUIStyle(),
                DescriptionStyle = new GUIStyle(),
                HandlerStyle = new GUIStyle(),
                ViewRect = new Rect(0, 0, 100, 100)
            };

            // 5) Test doubles + renderer under test
            _drawer = new FakeDrawer();
            _gfxGen = new FakeGraphicsGenerator();
            _renderer = new BlockRenderer(_drawer, _gfxGen);
        }

        [TearDown]
        public void TearDown()
        {
            _host.Dispose();
        }

        [Test]
        public void Render_DrawsOnlyBlocksInsideViewRect()
        {
            // Act
            _renderer.Render(_drawCtx);

            // Assert: only the inside block was drawn
            var drawnBlocks = _drawer.DrawCalls.Select(c => c.Block).ToList();
            Assert.That(drawnBlocks, Is.EqualTo(new[] { _insideBlock }));
            Assert.That(_drawer.DrawCalls.Count, Is.EqualTo(1));
        }

        [Test]
        public void Render_CallsGraphicsGeneratorForEachVisibleBlock()
        {
            // Act
            _renderer.Render(_drawCtx);

            // Assert: generator called exactly once, for the inside block
            Assert.That(_gfxGen.GeneratedFor, Is.EqualTo(new[] { _insideBlock }));
            Assert.That(_gfxGen.GeneratedFor.Count, Is.EqualTo(1));
        }

        [Test]
        public void Render_PassesGeneratedGraphicsIntoDrawer()
        {
            // Act
            _renderer.Render(_drawCtx);

            // The BlockGraphics instance returned by generator should match the one the drawer saw
            var returned = _gfxGen.ReturnedGraphics[0];
            var passed = _drawer.DrawCalls[0].Graphics;
            bool success = returned.Equals(passed);
            string errorMessage = "The BlockGraphics instance returned by the generator doesn't match the one the dreawer gets";
            Assert.IsTrue(success, errorMessage);
        }
    }
}