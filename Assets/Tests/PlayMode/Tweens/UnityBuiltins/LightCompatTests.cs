using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace TweeningTests.BuiltinCompat
{
    public class LightCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<Light, float> IntensityCase = new TweenCase<Light, float>
        {
            Name = "ShiftIntensityTo_Light",
            CreateTween = (adapter, light) => adapter.TweenIntensity(light, 2f, Duration),
            GetValue = l => l.intensity,
            SetValue = (l, v) => l.intensity = v,
            CreateComponent = go => go.AddComponent<Light>(),
            TargetValue = 2f
        };

        private static readonly TweenCase<Light, Color> ColorCase = new TweenCase<Light, Color>
        {
            Name = "ShiftColorTo_Light",
            CreateTween = (adapter, light) => adapter.FadeColor(light, Color.red, Duration),
            GetValue = l => l.color,
            SetValue = (l, c) => l.color = c,
            CreateComponent = go => go.AddComponent<Light>(),
            TargetValue = Color.red
        };

        private static readonly TweenCase<Light, float> RangeCase = new TweenCase<Light, float>
        {
            Name = "ShiftRangeTo_Light",
            CreateTween = (adapter, light) => adapter.FadeColor(light, 15f, Duration),
            GetValue = l => l.range,
            SetValue = (l, v) => l.range = v,
            CreateComponent = go => go.AddComponent<Light>(),
            TargetValue = 15f
        };

        private static readonly object[] FloatCases = { IntensityCase, RangeCase };
        private static readonly object[] ColorCases = { ColorCase };

        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] TweenCase<Light, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] TweenCase<Light, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.black);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = tc.GetValue(comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }

    }
}