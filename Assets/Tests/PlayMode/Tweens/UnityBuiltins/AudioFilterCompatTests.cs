
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using AtMycelia.AmaniTween;

namespace TweeningTests.BuiltinCompat
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

        [Test]
        public void Handle_IsValid_LowPass()
        {
            TweenCase<AudioLowPassFilter, float> tCase = LowPassCase;
            var comp = tCase.CreateComponent(_testGo);
            var handle = tCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        public static readonly object[] LowPassCases = { LowPassCase };
        public static readonly TweenCase<AudioLowPassFilter, float> LowPassCase = new TweenCase<AudioLowPassFilter, float>
        {
            Name = "ShiftLowPassCutoffTo",
            CreateTween = (adapter, filter) => adapter.FadeLowPassCutoff(filter, 500f, Duration),
            GetValue = filter => filter.cutoffFrequency,
            SetValue = (filter, newVal) => filter.cutoffFrequency = newVal,
            CreateComponent = go => go.AddComponent<AudioLowPassFilter>(),
            TargetValue = 500f
        };

        [Test]
        public void Handle_IsValid_Reverb()
        {
            TweenCase<AudioReverbFilter, float> tCase = ReverbCase;
            var comp = tCase.CreateComponent(_testGo);
            var handle = tCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<DefaultTweenHandle>(handle);
            Assert.IsNotNull(((DefaultTweenHandle)handle).Tween);
        }

        private static readonly TweenCase<AudioReverbFilter, float> ReverbCase = new TweenCase<AudioReverbFilter, float>
        {
            Name = "ShiftReverbLevelTo",
            CreateTween = (adapter, filter) => adapter.FadeReverbLevel(filter, -500f, Duration),
            GetValue = filter => filter.reverbLevel,
            SetValue = (filter, newVal) => filter.reverbLevel = newVal,
            CreateComponent = go => go.AddComponent<AudioReverbFilter>(),
            TargetValue = -500f
        };

        [UnityTest]
        public IEnumerator Tween_Completes_LowPass()
        {
            TweenCase<AudioLowPassFilter, float> tCase = LowPassCase;
            AudioLowPassFilter comp = tCase.CreateComponent(_testGo);
            tCase.SetValue(comp, 22000f); // start at max cutoff
            tCase.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tCase.TargetValue, tCase.GetValue(comp), Epsilon, tCase.Name, "The target and get value should be the same");
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Reverb()
        {
            TweenCase<AudioReverbFilter, float> tCase = ReverbCase;
            var comp = tCase.CreateComponent(_testGo);
            tCase.SetValue(comp, 0f); // start at neutral
            tCase.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tCase.TargetValue, tCase.GetValue(comp), Epsilon, tCase.Name);
        }
    }
}