using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace TweeningTests.BuiltinCompat
{
    public class MaterialCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<Renderer, Color> MatColorCase = new TweenCase<Renderer, Color>
        {
            Name = "ShiftColorTo",
            CreateTween = (adapter, rend) => adapter.FadeColor(rend.material, Color.magenta, Duration),
            GetValue = rend => rend.material.color,
            SetValue = (rend, c) => rend.material.color = c,
            CreateComponent = go => go.AddComponent<MeshRenderer>(),
            TargetValue = Color.magenta
        };

        private static readonly TweenCase<Renderer, float> MatFloatCase = new TweenCase<Renderer, float>
        {
            Name = "ShiftFloatTo",
            CreateTween = (adapter, rend) => adapter.TweenFloat(rend.material, "_Glossiness", 0.75f, Duration),
            GetValue = rend => rend.material.GetFloat("_Glossiness"),
            SetValue = (rend, v) => rend.material.SetFloat("_Glossiness", v),
            CreateComponent = go => go.AddComponent<MeshRenderer>(),
            TargetValue = 0.75f
        };

        private static readonly object[] ColorCases = { MatColorCase };
        private static readonly object[] FloatCases = { MatFloatCase };

        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] TweenCase<Renderer, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.black);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = tc.GetValue(comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }


        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] TweenCase<Renderer, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

    }
}