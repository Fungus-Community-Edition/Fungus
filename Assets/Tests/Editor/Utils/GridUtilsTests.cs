using NUnit.Framework;
using System.Collections.Generic;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    public class GridUtilsTests
    {
        const float Spacing = 100f;

        [Test]
        public void VerticalLines_ZeroOffset()
        {
            // scrollX = 0, viewWidth = 350
            IList<float> xPositions = GridUtils.GetVerticalLinePositions(0f, 350f, Spacing);
            // Expected at 0, 100, 200, 300
            Assert.AreEqual(new List<float> { 0f, 100f, 200f, 300f }, xPositions);
        }

        [Test]
        public void VerticalLines_HalfSpacingOffset()
        {
            // scrollX = 50 (half of 100), viewWidth = 350
            IList<float> xPositions = GridUtils.GetVerticalLinePositions(50f, 350f, Spacing);
            // offset = 50, then +100 until x < 350 ⇒ 50, 150, 250
            Assert.AreEqual(
                new List<float> { 50f, 150f, 250f },
                xPositions
            );
        }

        [Test]
        public void VerticalLines_OffsetAlignsExactly()
        {
            // scrollX = 100, viewWidth = 300
            IList<float> xPositions = GridUtils.GetVerticalLinePositions(100f, 300f, Spacing);
            // scrollX % spacing == 0 ⇒ offset = 0
            Assert.AreEqual(new List<float> { 0f, 100f, 200f }, xPositions);
        }

        [Test]
        public void HorizontalLines_ZeroOffset()
        {
            IList<float> yPositions = GridUtils.GetHorizontalLinePositions(0f, 250f, Spacing);
            Assert.AreEqual(new List<float> { 0f, 100f, 200f }, yPositions);
        }

        [Test]
        public void HorizontalLines_OffsetRemainder()
        {
            // scrollY = 30, viewHeight = 260
            IList<float> yPositions = GridUtils.GetHorizontalLinePositions(30f, 260f, Spacing);
            // offset = 70, then 170, 270 (but 270 ≥ 260 ⇒ excluded)
            Assert.AreEqual(new List<float> { 70f, 170f }, yPositions);
        }
    }
}