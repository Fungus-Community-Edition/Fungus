using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace BuiltinCompat
{
    public class LTI_AudioSourceCompatTests : LeanTweenAdapterTests
    {
        private static readonly LeanTweenCase<AudioSource, float> ShiftVolumeCase = new LeanTweenCase<AudioSource, float>
        {
            Name = "ShiftVolumeTo",
            CreateTween = (adapter, src) => adapter.FadeVolume(src, 50f, Duration), // 50% → 0.5f
            GetValue = src => src.volume,
            SetValue = (src, val) => src.volume = val,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = 0.5f
        };

        private static readonly LeanTweenCase<AudioSource, float> ShiftVolume01Case = new LeanTweenCase<AudioSource, float>
        {
            Name = "ShiftVolume01To",
            CreateTween = (adapter, src) => adapter.FadeVolume01(src, 0.5f, Duration),
            GetValue = src => src.volume,
            SetValue = (src, val) => src.volume = val,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = 0.5f
        };

        private static readonly LeanTweenCase<AudioSource, float> ShiftPitchCase = new LeanTweenCase<AudioSource, float>
        {
            Name = "ShiftPitchTo",
            CreateTween = (adapter, src) => adapter.FadePitch(src, 150f, Duration), // 150% → 1.5f
            GetValue = src => src.pitch,
            SetValue = (src, val) => src.pitch = val,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = 1.5f
        };

        private static readonly LeanTweenCase<AudioSource, float> ShiftPitchN33Case = new LeanTweenCase<AudioSource, float>
        {
            Name = "ShiftPitchN33To",
            CreateTween = (adapter, src) => adapter.FadePitchN33(src, -1.50f, Duration),
            GetValue = src => src.pitch,
            SetValue = (src, val) => src.pitch = val,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = -1.5f
        };

        private static readonly object[] AudioCases =
        {
            ShiftVolumeCase,
            ShiftVolume01Case,
            ShiftPitchCase,
            ShiftPitchN33Case
        };

        // --- Non-yield tests ---
        [TestCaseSource(nameof(AudioCases))]
        public void Handle_IsValid(LeanTweenCase<AudioSource, float> tCase)
        {
            var comp = tCase.CreateComponent(_testGo);
            var handle = tCase.CreateTween(_adapter, comp);
            Assert.IsInstanceOf<Amanita.LeanTweenIntegration.LeanTweenHandle>(handle);
            Assert.IsNotNull(handle);
        }

        [TestCaseSource(nameof(AudioCases))]
        public void Kill_DoesNotThrow(LeanTweenCase<AudioSource, float> tCase)
        {
            var comp = tCase.CreateComponent(_testGo);
            var handle = tCase.CreateTween(_adapter, comp);
            Assert.DoesNotThrow(() => handle.Kill());
            Assert.IsFalse(handle.IsPlaying);
        }

        // --- Yield tests ---
        [UnityTest]
        public IEnumerator Tween_CompletesWithExpectedValue(
            [ValueSource(nameof(AudioCases))] LeanTweenCase<AudioSource, float> tCase)
        {
            var comp = tCase.CreateComponent(_testGo);
            tCase.SetValue(comp, 0f); // start from zero for volume/pitch

            tCase.CreateTween(_adapter, comp);

            yield return new WaitForSeconds(Duration + 0.05f);

            var actual = tCase.GetValue(comp);
            Assert.AreEqual(tCase.TargetValue, actual, Epsilon, tCase.Name);
        }

        // --- Edge-case tests for 01 methods ---
        [UnityTest]
        public IEnumerator ShiftVolume01To_ZeroPercent_YieldsZero()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadeVolume01(src, 0f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(0f, src.volume, Epsilon);
        }

        [UnityTest]
        public IEnumerator ShiftVolume01To_HundredPercent_YieldsOne()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadeVolume01(src, 100f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(1f, src.volume, Epsilon);
        }

        [UnityTest]
        public IEnumerator ShiftPitchN33To_ZeroPercent_YieldsZero()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadePitchN33(src, 0f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(0f, src.pitch, Epsilon);
        }

        [UnityTest]
        public IEnumerator ShiftPitchN33To_ThreeHundredPercent_YieldsThree()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadePitchN33(src, 200f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(3f, src.pitch, Epsilon);
        }

        [UnityTest]
        public IEnumerator ShiftVolume01To_NegativePercent_ScalesToZero()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadeVolume01(src, -50f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(0f, src.volume, Epsilon);
        }

        [UnityTest]
        public IEnumerator ShiftPitchN33To_AboveThreeHundredPercent_ScalesUpToThree()
        {
            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadePitchN33(src, 300f, Duration);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(3f, src.pitch, Epsilon);
        }

        // --- Parameterized UnityTests ---
        [UnityTest]
        public IEnumerator ShiftVolume01To_EdgeCases(
            [ValueSource(nameof(Volume01EdgeCases))] object[] caseData)
        {
            float inputPercent = (float)caseData[0];
            float expected = (float)caseData[1];

            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadeVolume01(src, inputPercent, Duration);

            yield return new WaitForSeconds(Duration + 0.05f);

            Assert.AreEqual(expected, src.volume, Epsilon,
                $"FadeVolume01({inputPercent}%) should end at {expected}");
        }

        private static readonly object[] Volume01EdgeCases =
        {
            new object[] { 0f, 0f },     // 0% → 0.0
            new object[] { 100f, 1f },   // 100% → 1.0
            new object[] { -50f, 0f }    // negative → clamp to 0.0
        };

        [UnityTest]
        public IEnumerator ShiftPitchTo_EdgeCases(
            [ValueSource(nameof(PitchEdgeCases))] object[] caseData)
        {
            float inputPercent = (float)caseData[0];
            float expected = (float)caseData[1];

            var src = _testGo.AddComponent<AudioSource>();
            _adapter.FadePitch(src, inputPercent, Duration);

            yield return new WaitForSeconds(Duration + 0.05f);

            Assert.AreEqual(expected, src.pitch, Epsilon,
                $"FadePitch({inputPercent}%) should end at {expected}");
        }

        private static readonly object[] PitchEdgeCases =
        {
            new object[] { 0f, 0f },     // 0% → 0.0
            new object[] { 200f, 2f },   // 200% → 2.0
            new object[] { 300f, 3f }    // >200% → clamp to 3.0 in adapter
        };
    }
}