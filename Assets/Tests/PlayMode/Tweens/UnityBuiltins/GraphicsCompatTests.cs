
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.TestTools.Utils;
using AtMycelia.AmaniTween;

namespace TweeningTests.BuiltinCompat
{
    public class GraphicsCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<Graphic, Color> ShiftGraphicCase = new TweenCase<Graphic, Color>
        {
            Name = "ShiftColorTo_Graphic",
            CreateTween = (adapter, g) => adapter.FadeColor(g, Color.red, Duration),
            GetValue = g => g.color,
            SetValue = (g, c) => g.color = c,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = Color.red
        };

        private static readonly TweenCase<SpriteRenderer, Color> ShiftSpriteCase = new TweenCase<SpriteRenderer, Color>
        {
            Name = "ShiftColorTo_SpriteRenderer",
            CreateTween = (adapter, s) => adapter.FadeColor(s, Color.green, Duration),
            GetValue = s => s.color,
            SetValue = (s, c) => s.color = c,
            CreateComponent = go => go.AddComponent<SpriteRenderer>(),
            TargetValue = Color.green
        };

        private static readonly TweenCase<Graphic, Color> FadeGraphicCase = new TweenCase<Graphic, Color>
        {
            Name = "FadeTo_Graphic",
            CreateTween = (adapter, g) => adapter.FadeOpacity(g, 0.5f, Duration),
            GetValue = g => g.color,
            SetValue = (g, c) => g.color = c,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = new Color(1f, 1f, 1f, 0.5f)
        };

        private static readonly TweenCase<SpriteRenderer, Color> FadeSpriteCase = new TweenCase<SpriteRenderer, Color>
        {
            Name = "FadeTo_SpriteRenderer",
            CreateTween = (adapter, s) => adapter.FadeOpacity(s, 0.25f, Duration),
            GetValue = s => s.color,
            SetValue = (s, c) => s.color = c,
            CreateComponent = go => go.AddComponent<SpriteRenderer>(),
            TargetValue = new Color(1f, 1f, 1f, 0.25f)
        };

        private static readonly TweenCase<Image, float> ShiftFillCase = new TweenCase<Image, float>
        {
            Name = "ShiftFillTo_Image",
            CreateTween = (adapter, img) => adapter.ShiftFillTo(img, 0.75f, Duration),
            GetValue = img => img.fillAmount,
            SetValue = (img, v) => img.fillAmount = v,
            CreateComponent = go => go.AddComponent<Image>(),
            TargetValue = 0.75f
        };

        private static readonly object[] GraphicColorCases = { ShiftGraphicCase, FadeGraphicCase };
        private static readonly object[] SpriteColorCases = { ShiftSpriteCase, FadeSpriteCase };
        private static readonly object[] FillCases = { ShiftFillCase };

        [TestCaseSource(nameof(GraphicColorCases))]
        public void Handle_IsValid_Graphic(TweenCase<Graphic, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [TestCaseSource(nameof(SpriteColorCases))]
        public void Handle_IsValid_Sprite(TweenCase<SpriteRenderer, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [TestCaseSource(nameof(FillCases))]
        public void Handle_IsValid_Fill(TweenCase<Image, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [UnityTest]
        public IEnumerator Tween_CompletesWithExpectedValue_Graphic(
        [ValueSource(nameof(GraphicColorCases))] TweenCase<Graphic, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.white);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = tc.GetValue(comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_CompletesWithExpectedValue_Sprite(
            [ValueSource(nameof(SpriteColorCases))] TweenCase<SpriteRenderer, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.white);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = tc.GetValue(comp);
            var comparer = new ColorEqualityComparer(Epsilon);
            Assert.That(actual, Is.EqualTo(tc.TargetValue).Using(comparer), tc.Name);
        }


        [UnityTest]
        public IEnumerator Tween_CompletesWithExpectedValue_Fill(
            [ValueSource(nameof(FillCases))] TweenCase<Image, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

    }
}