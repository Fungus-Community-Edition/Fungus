using NUnit.Framework;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    public class CanvasUtilsTests
    {
        [Test]
        public void IsPointOverNode_NoZoomNoScroll_PointInside()
        {
            // Node at (10,20), size (100×50)
            var nodeRect = new Rect(10, 20, 100, 50);
            var scrollPos = Vector2.zero;
            float zoom = 1f;

            // Mouse at (15,30) → canvasMouse = (15,30)
            var mousePos = new Vector2(15, 30);

            bool hit = CanvasUtils.IsPointOverNode(
                nodeRect, scrollPos, zoom, mousePos);

            Assert.IsTrue(hit);
        }

        [Test]
        public void IsPointOverNode_WithScrollAndZoom_PointInside()
        {
            // Node at origin, size 50×50
            var nodeRect = new Rect(0, 0, 50, 50);
            // Scrolled right 10, down 20
            var scrollPos = new Vector2(10, 20);
            float zoom = 2f;

            // We want canvasMouse = within (10..60,20..70)
            // canvasMouse = mousePos / 2 → pick mousePos=(40,80) ⇒ canvasMouse=(20,40)
            var mousePos = new Vector2(40, 80);

            bool hit = CanvasUtils.IsPointOverNode(
                nodeRect, scrollPos, zoom, mousePos);

            Assert.IsTrue(hit);
        }

        [Test]
        public void IsPointOverNode_PointOutside_ReturnsFalse()
        {
            var nodeRect = new Rect(0, 0, 10, 10);
            var scrollPos = new Vector2(5, 5);
            float zoom = 1f;

            // canvasMouse = (20,20) → well outside the small rect
            var mousePos = new Vector2(20, 20);
            bool hit = CanvasUtils.IsPointOverNode(
                nodeRect, scrollPos, zoom, mousePos);

            Assert.IsFalse(hit);
        }

        [Test]
        public void CalculateScrollDelta_CenteredZoom_DeltaCorrect()
        {
            // View is 200×100 screen pixels
            var viewSize = new Vector2(200, 100);
            float oldZoom = 1f;
            float newZoom = 2f;
            // Pivot at center (0.5,0.5)
            var pivot = new Vector2(0.5f, 0.5f);

            // before = (200,100), after = (100,50) → diff = (100,50)
            // delta = -diff * pivot = (-50, -25)
            var delta = CanvasUtils.CalculateScrollDelta(
                viewSize, oldZoom, newZoom, pivot);

            Assert.AreEqual(new Vector2(-50f, -25f), delta);
        }

        [Test]
        public void CalculateScrollDelta_TopLeftPivot_ZeroDelta()
        {
            var viewSize = new Vector2(200, 100);
            float oldZoom = 1f;
            float newZoom = 3f;
            // Pivot at top-left (0,0)
            var pivot = Vector2.zero;

            var delta = CanvasUtils.CalculateScrollDelta(
                viewSize, oldZoom, newZoom, pivot);

            // no movement if pivot is fixed at origin
            Assert.AreEqual(Vector2.zero, delta);
        }
    }
}