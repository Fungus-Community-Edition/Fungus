using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using Amanita.EditorUtils;
using Amanita.VScripting.EditorUtils.FcWindow;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class BlockRendererTests
    {
        // Test double
        class FakeDrawer : IBlockDrawerUitk
        {
            public readonly List<Block> CreatedFor = new List<Block>();
            public readonly List<(Block Block, Button Button, float Zoom)> UpdateCalls
                = new List<(Block, Button, float)>();
            public readonly Dictionary<Block, Button> CreatedButtons = new Dictionary<Block, Button>();

            public Button CreateButton(Block block)
            {
                CreatedFor.Add(block);
                var button = new Button();
                CreatedButtons[block] = button;
                return button;
            }

            public void UpdateButton(Button button, Block block, float zoom)
            {
                UpdateCalls.Add((block, button, zoom));
            }
        }

        FlowchartContext _flowchartCtx;
        FakeFlowchartHost _host;
        Block _insideBlock;
        Block _outsideBlock;
        FakeDrawer _drawer;
        BlockRenderer _renderer;

        [SetUp]
        public void SetUp()
        {
            // 1) Create host + flowchart
            _host = new FakeFlowchartHost();
            _host.Init();
            var fc = _host.Flowchart;
            fc.Zoom = 1f;

            // 2) Create two blocks, one inside a 100×100 view, one outside
            _insideBlock = _host.CreateBlock(fc, Vector2.zero);
            _insideBlock._NodeRect = new Rect(10, 10, 20, 20);

            _outsideBlock = _host.CreateBlock(fc, Vector2.zero);
            _outsideBlock._NodeRect = new Rect(200, 200, 20, 20);

            // 3) Prepare FlowchartContext
            _flowchartCtx = new FlowchartContext
            {
                Flowchart = fc,
                Position = new Rect(0, 0, 100, 100),  // window size in screen-space
                FcHost = _host,
            };

            // 4) Test double + renderer under test
            _drawer = new FakeDrawer();
            _renderer = new BlockRenderer(_flowchartCtx, _drawer);
        }

        [TearDown]
        public void TearDown()
        {
            _renderer.Dispose();
            _host.Dispose();
        }

        [Test]
        public void RefreshBlocks_CreatesButtonsForAllBlocks()
        {
            // Act
            _renderer.RefreshBlocks();

            // Assert: both blocks were created
            CollectionAssert.AreEquivalent(new[] { _insideBlock, _outsideBlock }, _drawer.CreatedFor);
            Assert.That(_renderer.childCount, Is.EqualTo(2));
        }

        [Test]
        public void RefreshBlocks_UpdatesButtonsForAllBlocks()
        {
            // Act
            _renderer.RefreshBlocks();

            // Assert: both blocks received updates
            var updatedBlocks = _drawer.UpdateCalls.Select(c => c.Block).Distinct().ToList();
            CollectionAssert.AreEquivalent(new[] { _insideBlock, _outsideBlock }, updatedBlocks);
        }

        [Test]
        public void RefreshBlocks_PassesCreatedButtonsIntoUpdater()
        {
            // Act
            _renderer.RefreshBlocks();

            // Assert: update calls use the same button created for each block
            foreach (var pair in _drawer.CreatedButtons)
            {
                Block block = pair.Key;
                Button createdButton = pair.Value;

                bool found = _drawer.UpdateCalls.Any(c =>
                    ReferenceEquals(c.Block, block) && ReferenceEquals(c.Button, createdButton));

                Assert.IsTrue(found, $"Expected update calls for block '{block.BlockName}' to use its created button.");
            }
        }
    }
}