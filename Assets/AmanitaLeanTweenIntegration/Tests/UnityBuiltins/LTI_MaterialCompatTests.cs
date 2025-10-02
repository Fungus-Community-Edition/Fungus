using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace BuiltinCompat
{
    public class LTI_MaterialCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<Renderer, Color> FadeMatColorCase = new LeanTweenCase<Renderer, Color>
        {
            Name = "ShiftColorTo_Material",
            CreateTween = (adapter, r) =>
            {
                var mat = r.sharedMaterial;
                return adapter.FadeColor(r.gameObject, mat, Color.yellow, Duration);
            },
            GetValue = r => r.sharedMaterial.color,
            SetValue = (r, c) => r.sharedMaterial.color = c,
            CreateComponent = go =>
            {
                var rend = go.AddComponent<MeshRenderer>();
                rend.sharedMaterial = new Material(Shader.Find("Standard"));
                return rend;
            },
            TargetValue = Color.yellow
        };

        private static readonly LeanTweenCase<Renderer, float> FloatMatCase = new LeanTweenCase<Renderer, float>
        {
            Name = "ShiftFloatTo_Material",
            CreateTween = (adapter, r) =>
            {
                var mat = r.sharedMaterial;
                return adapter.TweenFloat(r.gameObject, mat, "_Glossiness", 0.2f, Duration);
            },
            GetValue = r => r.sharedMaterial.GetFloat("_Glossiness"),
            SetValue = (r, v) => r.sharedMaterial.SetFloat("_Glossiness", v),
            CreateComponent = go =>
            {
                var rend = go.AddComponent<MeshRenderer>();
                rend.sharedMaterial = new Material(Shader.Find("Standard"));
                return rend;
            },
            TargetValue = 0.2f
        };

        // Wrap single-case fields in arrays so NUnit's ValueSource can iterate them
        private static readonly object[] ColorCases = { FadeMatColorCase };
        private static readonly object[] FloatCases = { FloatMatCase };

        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] LeanTweenCase<Renderer, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.black);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            var actual = tc.GetValue(comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(comparer));
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] LeanTweenCase<Renderer, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 1f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}