
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.AmaniTween;

namespace TweeningTests.BuiltinCompat
{
    public class CanvasGroupCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<CanvasGroup, float> FadeCase = new TweenCase<CanvasGroup, float>
        {
            Name = "FadeTo_CanvasGroup",
            CreateTween = (adapter, cg) => adapter.FadeOpacity(cg, 0.25f, Duration),
            GetValue = cg => cg.alpha,
            SetValue = (cg, v) => cg.alpha = v,
            CreateComponent = go => go.AddComponent<CanvasGroup>(),
            TargetValue = 0.25f
        };

        private static readonly object[] Cases = { FadeCase };

        [TestCaseSource(nameof(Cases))]
        public void Handle_IsValid(TweenCase<CanvasGroup, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [UnityTest]
        public IEnumerator Tween_Completes([ValueSource(nameof(Cases))] TweenCase<CanvasGroup, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 1f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}