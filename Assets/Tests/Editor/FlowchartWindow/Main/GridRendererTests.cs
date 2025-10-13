using Amanita.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    [TestFixture]
    public class GridRendererTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            _drawer = new FakeLineDrawer();
            _renderer = new GridRenderer(_drawer);

            _gridCtx = new DrawGridContext
            {
                GridLineColor = _gridLineColor,
                GridLineSpacingSize = _gridLineSpacingSize
            };
        }

        protected FakeLineDrawer _drawer;
        protected GridRenderer _renderer;
        protected DrawGridContext _gridCtx;
        protected readonly Color _gridLineColor = Color.red;
        protected readonly int _gridLineSpacingSize = 50;

        [Test]
        public void Draw_CallsDrawLine_ForExpectedPositions()
        {
            // Arrange
            var spacing = _gridCtx.GridLineSpacingSize;
            float width = ctx.Position.width / ctx.Flowchart.Zoom;
            float height = ctx.Position.height / ctx.Flowchart.Zoom;

            // Compute exactly what lines GridRenderer should draw
            var expectedXs = GridUtils.GetVerticalLinePositions(
                ctx.Flowchart.ScrollPos.x, width, spacing
            );
            var expectedYs = GridUtils.GetHorizontalLinePositions(
                ctx.Flowchart.ScrollPos.y, height, spacing
            );

            // Act
            _renderer.Draw(ctx, _gridCtx);

            // Assert vertical lines
            foreach (float x in expectedXs)
            {
                var line = (new Vector2(x, 0), new Vector2(x, height));
                Assert.Contains(line, _drawer.LinesDrawn,
                    $"Expected a vertical line at x={x} from y=0→{height}");
            }

            // Assert horizontal lines
            foreach (float y in expectedYs)
            {
                var line = (new Vector2(0, y), new Vector2(width, y));
                Assert.Contains(line, _drawer.LinesDrawn,
                    $"Expected a horizontal line at y={y} from x=0→{width}");
            }
        }

        [Test]
        public void Draw_PreservesDrawerColor()
        {
            // Arrange
            _drawer.Color = Color.blue;

            // Act
            _renderer.Draw(ctx, _gridCtx);

            // Assert original color restored
            Assert.AreEqual(Color.blue, _drawer.Color);
        }
    }

}