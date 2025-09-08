
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Amanita.Tweening.BuiltinCompat
{
    public class AudioFilterCompatTests : DefaultAdapterTests
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
            CreateTween = (adapter, filter) => adapter.ShiftLowPassCutoffTo(filter, 500f, Duration),
            GetValue = filter => filter.cutoffFrequency,
            SetValue = (filter, newVal) => filter.cutoffFrequency = newVal,
            CreateComponent = go => go.AddComponent<AudioLowPassFilter>(),
            TargetValue = 500f
        };

        private static readonly TweenCase<AudioReverbFilter, float> ReverbCase = new TweenCase<AudioReverbFilter, float>
        {
            Name = "ShiftReverbLevelTo",
            CreateTween = (adapter, filter) => adapter.ShiftReverbLevelTo(filter, -500f, Duration),
            GetValue = filter => filter.reverbLevel,
            SetValue = (filter, newVal) => filter.reverbLevel = newVal,
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
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [TestCaseSource(nameof(ReverbCases))]
        public void Handle_IsValid_Reverb(TweenCase<AudioReverbFilter, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            var handle = tc.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_LowPass([ValueSource(nameof(LowPassCases))] TweenCase<AudioLowPassFilter, float> tCase)
        {
            AudioLowPassFilter comp = tCase.CreateComponent(_testGo);
            tCase.SetValue(comp, 22000f); // start at max cutoff
            tCase.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tCase.TargetValue, tCase.GetValue(comp), Epsilon, tCase.Name, "The target and get value should be the same");
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Reverb([ValueSource(nameof(ReverbCases))] TweenCase<AudioReverbFilter, float> tCase)
        {
            var comp = tCase.CreateComponent(_testGo);
            tCase.SetValue(comp, 0f); // start at neutral
            tCase.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tCase.TargetValue, tCase.GetValue(comp), Epsilon, tCase.Name);
        }
    }
}