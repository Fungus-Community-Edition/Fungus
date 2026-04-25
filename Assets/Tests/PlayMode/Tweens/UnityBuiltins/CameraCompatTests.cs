
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.AmaniTween;

namespace TweeningTests.BuiltinCompat
{
    public class CameraCompatTests : DefaultAdapterTests
    {
        private static readonly TweenCase<Camera, float> FOVCase = new TweenCase<Camera, float>
        {
            Name = "ShiftFieldOfViewTo",
            CreateTween = (adapter, cam) => adapter.TweenFOV(cam, 60f, Duration),
            GetValue = cam => cam.fieldOfView,
            SetValue = (cam, v) => cam.fieldOfView = v,
            CreateComponent = go => go.AddComponent<Camera>(),
            TargetValue = 60f
        };

        private static readonly TweenCase<Camera, float> OrthoSizeCase = new TweenCase<Camera, float>
        {
            Name = "ShiftOrthographicSizeTo",
            CreateTween = (adapter, cam) => adapter.TweenOrthoSize(cam, 5f, Duration),
            GetValue = cam => cam.orthographicSize,
            SetValue = (cam, v) => cam.orthographicSize = v,
            CreateComponent = go => go.AddComponent<Camera>(),
            TargetValue = 5f
        };

        private static readonly TweenCase<Camera, Color> BgColorCase = new TweenCase<Camera, Color>
        {
            Name = "ShiftBackgroundColorTo",
            CreateTween = (adapter, cam) => adapter.FadeBackgroundColor(cam, Color.blue, Duration),
            GetValue = cam => cam.backgroundColor,
            SetValue = (cam, c) => cam.backgroundColor = c,
            CreateComponent = go => go.AddComponent<Camera>(),
            TargetValue = Color.blue
        };

        private static readonly object[] FloatCases = { FOVCase, OrthoSizeCase };
        private static readonly object[] ColorCases = { BgColorCase };

        [TestCaseSource(nameof(FloatCases))]
        public void Handle_IsValid_Float<T>(TweenCase<T, float> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [TestCaseSource(nameof(ColorCases))]
        public void Handle_IsValid_Color<T>(TweenCase<T, Color> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] TweenCase<Camera, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] TweenCase<Camera, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.black);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            var actual = tc.GetValue(comp);
            Assert.AreEqual(tc.TargetValue.r, actual.r, Epsilon, tc.Name);
            Assert.AreEqual(tc.TargetValue.g, actual.g, Epsilon, tc.Name);
            Assert.AreEqual(tc.TargetValue.b, actual.b, Epsilon, tc.Name);
        }
    }
}