using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace BuiltinCompat
{
    public class LTI_CameraCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<Camera, float> FOVCase = new LeanTweenCase<Camera, float>
        {
            Name = "ShiftFieldOfViewTo",
            CreateTween = (adapter, cam) => adapter.TweenFOV(cam, 60f, Duration),
            GetValue = cam => cam.fieldOfView,
            SetValue = (cam, v) => cam.fieldOfView = v,
            CreateComponent = go => go.AddComponent<Camera>(),
            TargetValue = 60f
        };

        private static readonly LeanTweenCase<Camera, float> OrthoSizeCase = new LeanTweenCase<Camera, float>
        {
            Name = "ShiftOrthographicSizeTo",
            CreateTween = (adapter, cam) => adapter.TweenOrthoSize(cam, 5f, Duration),
            GetValue = cam => cam.orthographicSize,
            SetValue = (cam, v) => cam.orthographicSize = v,
            CreateComponent = go => go.AddComponent<Camera>(),
            TargetValue = 5f
        };

        private static readonly LeanTweenCase<Camera, Color> BgColorCase = new LeanTweenCase<Camera, Color>
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
        public void Handle_IsValid_Float<T>(LeanTweenCase<T, float> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [TestCaseSource(nameof(ColorCases))]
        public void Handle_IsValid_Color<T>(LeanTweenCase<T, Color> tc) where T : Component
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Float([ValueSource(nameof(FloatCases))] LeanTweenCase<Camera, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 1f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Color([ValueSource(nameof(ColorCases))] LeanTweenCase<Camera, Color> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, Color.black);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp));
        }
    }
}