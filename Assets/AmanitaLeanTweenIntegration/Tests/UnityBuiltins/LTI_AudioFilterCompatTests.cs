using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace BuiltinCompat
{
    public class LTI_AudioFilterCompatTests : LeanTweenAdapterTests
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _audioSource = _testGo.AddComponent<AudioSource>();
        }

        protected AudioSource _audioSource;

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            _audioSource = null;
        }

        private static readonly LeanTweenCase<AudioLowPassFilter, float> LowPassCase = new LeanTweenCase<AudioLowPassFilter, float>
        {
            Name = "ShiftLowPassCutoffTo",
            CreateTween = (adapter, f) => adapter.FadeLowPassCutoff(f, 500f, Duration),
            GetValue = f => f.cutoffFrequency,
            SetValue = (f, v) => f.cutoffFrequency = v,
            CreateComponent = go => go.AddComponent<AudioLowPassFilter>(),
            TargetValue = 500f
        };

        private static readonly LeanTweenCase<AudioReverbFilter, float> ReverbCase = new LeanTweenCase<AudioReverbFilter, float>
        {
            Name = "ShiftReverbLevelTo",
            CreateTween = (adapter, f) => adapter.FadeReverbLevel(f, -500f, Duration),
            GetValue = f => f.reverbLevel,
            SetValue = (f, v) => f.reverbLevel = v,
            CreateComponent = go => go.AddComponent<AudioReverbFilter>(),
            TargetValue = -500f
        };

        private static readonly object[] LowPassCases = { LowPassCase };
        private static readonly object[] ReverbCases = { ReverbCase };

        [TestCaseSource(nameof(LowPassCases))]
        public void Handle_IsValid_LowPass(LeanTweenCase<AudioLowPassFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [TestCaseSource(nameof(ReverbCases))]
        public void Handle_IsValid_Reverb(LeanTweenCase<AudioReverbFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_LowPass([ValueSource(nameof(LowPassCases))] LeanTweenCase<AudioLowPassFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 100f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Reverb([ValueSource(nameof(ReverbCases))] LeanTweenCase<AudioReverbFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}