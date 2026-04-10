using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using UnityEngine.UIElements;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class BlockRendererTests
    {
        // Test double
        class FakeDrawer : IBlockDrawerUitk
        {
            private readonly VisualTreeAsset blockTemplate;
            private readonly StyleSheet baseStyleSheet;
            private readonly StyleSheet selectedStyleSheet;

            public FakeDrawer()
            {
                FlowchartWindowConfig config = HyphlowEditorSysAssets.FcwConfig;
                blockTemplate = config.BlockUxml;
                baseStyleSheet = config.BlockStyleSheet;
                selectedStyleSheet = config.SelectedBlockStyleSheet;
            }

            public readonly List<Block> CreatedFor = new List<Block>();
            public readonly List<(Block Block, BlockButton Button, float Zoom)> UpdateCalls
                = new List<(Block, BlockButton, float)>();
            public readonly Dictionary<Block, BlockButton> CreatedButtons = new Dictionary<Block, BlockButton>();

            public BlockButton CreateButton(Block block)
            {
                CreatedFor.Add(block);
                var button = new BlockButton(new BlockGraphicsGenerator());
                button.Initialize(block, blockTemplate, baseStyleSheet, selectedStyleSheet);
                CreatedButtons[block] = button;
                return button;
            }

            public void UpdateButton(BlockButton button, Block block, float zoom)
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
                BlockButton createdButton = pair.Value;

                bool found = _drawer.UpdateCalls.Any(c =>
                    ReferenceEquals(c.Block, block) && ReferenceEquals(c.Button, createdButton));

                Assert.IsTrue(found, $"Expected update calls for block '{block.BlockName}' to use its created button.");
            }
        }
    }
}