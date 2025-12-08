using NUnit.Framework;
using UnityEngine;
using Amanita.VScripting.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    public class ConnectionDrawerBezierTests
    {
        [Test]
        public void GetPointOnCurve_AtStart_ReturnsFromAnchor()
        {
            var fromAnchor = new Vector2(1, 2);
            var fromControl = new Vector2(3, 4);
            var toControl = new Vector2(5, 6);
            var toAnchor = new Vector2(7, 8);

            // progress = 0 → should equal fromAnchor
            var result = ConnectionDrawer.GetPointOnCurve(fromAnchor, fromControl, toControl, toAnchor, 0f);

            Assert.AreEqual(fromAnchor, result);
        }

        [Test]
        public void GetPointOnCurve_AtEnd_ReturnsToAnchor()
        {
            var fromAnchor = new Vector2(-1, -2);
            var fromControl = new Vector2(-3, -4);
            var toControl = new Vector2(-5, -6);
            var toAnchor = new Vector2(-7, -8);

            // progress = 1 → should equal toAnchor
            var result = ConnectionDrawer.GetPointOnCurve(fromAnchor, fromControl, toControl, toAnchor, 1f);

            Assert.AreEqual(toAnchor, result);
        }

        [Test]
        public void GetPointOnCurve_LinearCase_InterpolatesLinearly()
        {
            // when control points == anchors, curve is straight line
            var A = new Vector2(0, 0);
            var B = new Vector2(10, 0);

            var result50 = ConnectionDrawer
                .GetPointOnCurve(A, A, B, B, 0.5f);

            // halfway between A and B
            Assert.AreEqual(new Vector2(5, 0), result50);
        }

        [Test]
        public void ArrowDirection_IsForwardAlongCurve()
        {
            // Simple diagonal from (0,0) to (10,10)
            var A = new Vector2(0, 0);
            var B = new Vector2(10, 10);

            // Make it a straight line by matching controls to anchors
            var ctrlA = A;
            var ctrlB = B;

            float t1 = 0.7f;
            float t2 = t1 + 0.1f;

            // Sample mid and a little ahead
            var mid = ConnectionDrawer.GetPointOnCurve(A, ctrlA, ctrlB, B, t1);
            var ahead = ConnectionDrawer.GetPointOnCurve(A, ctrlA, ctrlB, B, t2);

            // Direction of travel
            var dir = (ahead - mid).normalized;

            // Expected direction is (B–A).normalized
            var expected = (B - A).normalized;

            // Use dot‐product to assert they’re nearly identical
            float alignment = Vector2.Dot(dir, expected);
            Assert.That(alignment, Is.GreaterThan(0.999f),
                $"dir={dir} expected≈{expected}, dot={alignment}");
        }
    }
}