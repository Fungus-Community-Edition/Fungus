using Amanita.DOTweenIntegration;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace BuiltinCompat
{
    public class AudioFilterCompatTests : DoTweenAdapterTests
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


        private static readonly TweenCase<AudioLowPassFilter, float> LowPassCase = new TweenCase<AudioLowPassFilter, float>
        {
            Name = "ShiftLowPassCutoffTo",
            CreateTween = (adapter, f) => adapter.ShiftLowPassCutoffTo(f, 500f, Duration),
            GetValue = f => f.cutoffFrequency,
            SetValue = (f, v) => f.cutoffFrequency = v,
            CreateComponent = go => go.AddComponent<AudioLowPassFilter>(),
            TargetValue = 500f
        };

        private static readonly TweenCase<AudioReverbFilter, float> ReverbCase = new TweenCase<AudioReverbFilter, float>
        {
            Name = "ShiftReverbLevelTo",
            CreateTween = (adapter, f) => adapter.ShiftReverbLevelTo(f, -500f, Duration),
            GetValue = f => f.reverbLevel,
            SetValue = (f, v) => f.reverbLevel = v,
            CreateComponent = go => go.AddComponent<AudioReverbFilter>(),
            TargetValue = -500f
        };

        private static readonly object[] LowPassCases = { LowPassCase };
        private static readonly object[] ReverbCases = { ReverbCase };

        [TestCaseSource(nameof(LowPassCases))]
        public void Handle_IsValid_LowPass(TweenCase<AudioLowPassFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DOTweenHandle>(handle);
            Assert.IsNotNull(((DOTweenHandle)handle).Tween);
        }

        [TestCaseSource(nameof(ReverbCases))]
        public void Handle_IsValid_Reverb(TweenCase<AudioReverbFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DOTweenHandle>(handle);
            Assert.IsNotNull(((DOTweenHandle)handle).Tween);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_LowPass([ValueSource(nameof(LowPassCases))] TweenCase<AudioLowPassFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 22000f); // start at max cutoff
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name, "The target and get value should be the same");
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Reverb([ValueSource(nameof(ReverbCases))] TweenCase<AudioReverbFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 0f); // start at neutral
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}