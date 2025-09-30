using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.TestTools.Utils;

namespace BuiltinCompat
{
    public class LTI_GraphicsCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<Graphic, Color> ShiftGraphicCase = new LeanTweenCase<Graphic, Color>
        {
            Name = "ShiftColorTo_Graphic",
            CreateTween = (adapter, g) => adapter.FadeColor(g, Color.red, Duration),
            GetValue = g => g.color,
            SetValue = (g, c) => g.color = c,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = Color.red
        };

        private static readonly LeanTweenCase<SpriteRenderer, Color> ShiftSpriteCase = new LeanTweenCase<SpriteRenderer, Color>
        {
            Name = "ShiftColorTo_SpriteRenderer",
            CreateTween = (adapter, s) => adapter.FadeColor(s, Color.green, Duration),
            GetValue = s => s.color,
            SetValue = (s, c) => s.color = c,
            CreateComponent = go => go.AddComponent<SpriteRenderer>(),
            TargetValue = Color.green
        };

        private static readonly LeanTweenCase<Graphic, Color> FadeGraphicCase = new LeanTweenCase<Graphic, Color>
        {
            Name = "FadeTo_Graphic",
            CreateTween = (adapter, g) => adapter.FadeOpacity(g, 0.5f, Duration),
            GetValue = g => g.color,
            SetValue = (g, c) => g.color = c,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = new Color(1f, 1f, 1f, 0.5f)
        };

        private static readonly LeanTweenCase<SpriteRenderer, Color> FadeSpriteCase = new LeanTweenCase<SpriteRenderer, Color>
        {
            Name = "FadeTo_SpriteRenderer",
            CreateTween = (adapter, s) => adapter.FadeOpacity(s, 0.25f, Duration),
            GetValue = s => s.color,
            SetValue = (s, c) => s.color = c,
            CreateComponent = go => go.AddComponent<SpriteRenderer>(),
            TargetValue = new Color(1f, 1f, 1f, 0.25f)
        };

        private static readonly LeanTweenCase<Image, float> ShiftFillCase = new LeanTweenCase<Image, float>
        {
            Name = "ShiftFillTo_Image",
            CreateTween = (adapter, img) => adapter.ShiftFillTo(img, 0.75f, Duration),
            GetValue = img => img.fillAmount,
            SetValue = (img, v) => img.fillAmount = v,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = 0.75f
        };

        private static readonly object[] ColorCases = { ShiftGraphicCase, ShiftSpriteCase, FadeGraphicCase, FadeSpriteCase };
        private static readonly object[] FloatCases = { ShiftFillCase };

        [TestCaseSource(nameof(ColorCases))]
        public void Handle_IsValid_Color<T>(LeanTweenCase<T, Color> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [TestCaseSource(nameof(FloatCases))]
        public void Handle_IsValid_Float<T>(LeanTweenCase<T, float> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        // Accept the raw object from the ValueSource and use dynamic dispatch inside the test.
        // This avoids NUnit attempting to cast the source element to a mismatched generic type,
        // which caused the ArgumentException when the case element's generic parameter didn't
        // exactly match the method parameter type.
        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] object tcObj)
        {
            dynamic tc = tcObj;
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue((dynamic)comp, new Color(1f, 1f, 1f, 0f));
            tc.CreateTween(_adapter, (dynamic)comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            var actual = tc.GetValue((dynamic)comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo((Color)tc.TargetValue).Using(comparer));
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] LeanTweenCase<Image, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}