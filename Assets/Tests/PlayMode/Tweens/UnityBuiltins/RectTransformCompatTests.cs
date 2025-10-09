using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils; // equality comparers

namespace TweeningTests.BuiltinCompat
{
    public class RectTransformCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<RectTransform, Vector2> AnchoredPosCase = new TweenCase<RectTransform, Vector2>
        {
            Name = "ShiftAnchoredPositionTo",
            CreateTween = (adapter, rt) => adapter.TweenAnchoredPosition(rt, new Vector2(50f, 25f), Duration),
            GetValue = rt => rt.anchoredPosition,
            SetValue = (rt, v) => rt.anchoredPosition = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = new Vector2(50f, 25f)
        };

        private static readonly TweenCase<RectTransform, Vector2> SizeDeltaCase = new TweenCase<RectTransform, Vector2>
        {
            Name = "ShiftSizeDeltaTo",
            CreateTween = (adapter, rt) => adapter.TweenSizeDelta(rt, new Vector2(200f, 100f), Duration),
            GetValue = rt => rt.sizeDelta,
            SetValue = (rt, v) => rt.sizeDelta = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = new Vector2(200f, 100f)
        };

        private static readonly TweenCase<RectTransform, Quaternion> RotationCase = new TweenCase<RectTransform, Quaternion>
        {
            Name = "RotateTo",
            CreateTween = (adapter, rt) => adapter.RotateTo(rt, Quaternion.Euler(0f, 0f, 90f), Duration),
            GetValue = rt => rt.rotation,
            SetValue = (rt, v) => rt.rotation = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = Quaternion.Euler(0f, 0f, 90f)
        };

        private static readonly TweenCase<RectTransform, Vector3> ScaleCase = new TweenCase<RectTransform, Vector3>
        {
            Name = "ScaleTo",
            CreateTween = (adapter, rt) => adapter.ScaleTo(rt, new Vector3(2f, 2f, 2f), Duration),
            GetValue = rt => rt.localScale,
            SetValue = (rt, v) => rt.localScale = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = new Vector3(2f, 2f, 2f)
        };

        private static readonly object[] Vector2Cases = { AnchoredPosCase, SizeDeltaCase };
        private static readonly object[] QuaternionCases = { RotationCase };
        private static readonly object[] Vector3Cases = { ScaleCase };

        [UnityTest]
        public IEnumerator Tween_Completes_Vector2([ValueSource(nameof(Vector2Cases))] TweenCase<RectTransform, Vector2> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Vector2.zero);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            // Use constructor comparer instead of .Within(...)
            var comparer = new Vector2EqualityComparer(Epsilon);
            Assert.That(tc.GetValue(comp), Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Quaternion([ValueSource(nameof(QuaternionCases))] TweenCase<RectTransform, Quaternion> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Quaternion.identity);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            // Option A: angle tolerance (recommended)
            var angle = Quaternion.Angle(tc.GetValue(comp), tc.TargetValue);
            Assert.LessOrEqual(angle, 1f, $"{tc.Name} angle diff {angle} > 1°");

            // Option B: comparer (if you prefer component-wise tolerance)
            // var comparer = new QuaternionEqualityComparer(Epsilon);
            // Assert.That(tc.GetValue(comp), Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Vector3([ValueSource(nameof(Vector3Cases))] TweenCase<RectTransform, Vector3> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Vector3.one);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            var comparer = new Vector3EqualityComparer(Epsilon);
            Assert.That(tc.GetValue(comp), Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }
    }
}