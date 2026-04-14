using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;
using Block = AtMycelia.Hyphlow.Block;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class GridRendererTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _gridCtx = new DrawGridContext
            {
                GridLineColor = _gridLineColor,
                GridLineSpacingSize = _gridLineSpacingSize
            };

            _renderer = new GridRenderer(ctx, _gridCtx);
        }

        protected GridRenderer _renderer;
        protected DrawGridContext _gridCtx;
        protected readonly Color _gridLineColor = Color.red;
        protected readonly int _gridLineSpacingSize = 50;

        [Test]
        public void OnFlowchartChanged_ResetsCachedState()
        {
            // Arrange
            SetPrivateField(_renderer, "cachedScrollPosition", new Vector2(10f, 20f));
            SetPrivateField(_renderer, "cachedZoom", 2f);

            // Act
            _renderer.OnFlowchartChanged(flowchart, flowchart);

            // Assert
            Vector2 cachedScrollPosition = GetPrivateField<Vector2>(_renderer, "cachedScrollPosition");
            float cachedZoom = GetPrivateField<float>(_renderer, "cachedZoom");

            Assert.That(float.IsNaN(cachedScrollPosition.x), Is.True);
            Assert.That(float.IsNaN(cachedScrollPosition.y), Is.True);
            Assert.That(float.IsNaN(cachedZoom), Is.True);
        }

        [Test]
        public void OnBlockSelected_UpdatesLastSelection()
        {
            // Arrange
            Block selectedBlock = blocks[0];

            // Act
            _renderer.OnBlockSelected(selectedBlock);

            // Assert
            Block lastSelectedBlock = GetPrivateField<Block>(_renderer, "lastSelectedBlock");
            IList<Block> lastBlocksSelected = GetPrivateField<IList<Block>>(_renderer, "lastBlocksSelected");

            Assert.AreSame(selectedBlock, lastSelectedBlock);
            CollectionAssert.AreEqual(new Block[] { selectedBlock }, lastBlocksSelected);
        }

        [Test]
        public void OnMultiBlocksSelected_TracksSelectionList()
        {
            // Arrange
            IList<Block> selectedBlocks = new List<Block> { blocks[0], blocks[1] };

            // Act
            _renderer.OnMultiBlocksSelected(selectedBlocks);

            // Assert
            Block lastSelectedBlock = GetPrivateField<Block>(_renderer, "lastSelectedBlock");
            IList<Block> lastBlocksSelected = GetPrivateField<IList<Block>>(_renderer, "lastBlocksSelected");

            Assert.IsNull(lastSelectedBlock);
            CollectionAssert.AreEqual(selectedBlocks, lastBlocksSelected);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
            return (T)field.GetValue(target);
        }

        private static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Field '{fieldName}' was not found.");
            field.SetValue(target, value);
        }
    }
}