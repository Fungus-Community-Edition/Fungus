using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace BuiltinCompat
{
    public class LTI_RectTransformCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<RectTransform, Vector2> AnchoredCase = new LeanTweenCase<RectTransform, Vector2>
        {
            Name = "ShiftAnchoredPositionTo",
            CreateTween = (adapter, rt) => adapter.TweenAnchoredPosition(rt, new Vector2(2f, 3f), Duration),
            GetValue = rt => rt.anchoredPosition,
            SetValue = (rt, v) => rt.anchoredPosition = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = new Vector2(2f, 3f)
        };

        private static readonly LeanTweenCase<RectTransform, Vector2> SizeCase = new LeanTweenCase<RectTransform, Vector2>
        {
            Name = "ShiftSizeDeltaTo",
            CreateTween = (adapter, rt) => adapter.TweenSizeDelta(rt, new Vector2(4f, 5f), Duration),
            GetValue = rt => rt.sizeDelta,
            SetValue = (rt, v) => rt.sizeDelta = v,
            CreateComponent = go => go.AddComponent<RectTransform>(),
            TargetValue = new Vector2(4f, 5f)
        };

        private static readonly object[] Cases = { AnchoredCase, SizeCase };

        [TestCaseSource(nameof(Cases))]
        public void Handle_IsValid(LeanTweenCase<RectTransform, Vector2> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [UnityTest]
        public IEnumerator Tween_Completes([ValueSource(nameof(Cases))] LeanTweenCase<RectTransform, Vector2> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Vector2.zero);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            var actual = tc.GetValue(comp);
            var vec2 = new Vector2EqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(vec2));
        }
    }
}