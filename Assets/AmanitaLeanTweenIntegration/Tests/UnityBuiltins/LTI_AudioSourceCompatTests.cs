using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace BuiltinCompat
{
    public class LTI_AudioSourceCompatTests : LeanTweenAdapterTests
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

        private static readonly LeanTweenCase<AudioSource, float> VolumeCase = new LeanTweenCase<AudioSource, float>
        {
            Name = "FadeTo_AudioSource_Volume",
            CreateTween = (adapter, a) => adapter.FadeVolume01(a, 0.2f, Duration),
            GetValue = a => a.volume,
            SetValue = (a, v) => a.volume = v,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = 0.2f
        };

        private static readonly LeanTweenCase<AudioSource, float> PitchCase = new LeanTweenCase<AudioSource, float>
        {
            Name = "FadePitchTo_AudioSource",
            CreateTween = (adapter, a) => adapter.FadePitch(a, 150f, Duration),
            GetValue = a => a.pitch,
            SetValue = (a, v) => a.pitch = v,
            CreateComponent = go => go.AddComponent<AudioSource>(),
            TargetValue = 1.5f // FadePitch divides by 100 in adapter implementation
        };

        // ValueSource expects an IEnumerable; wrap cases in arrays so NUnit can iterate them
        private static readonly object[] VolumeCases = { VolumeCase };
        private static readonly object[] PitchCases = { PitchCase };

        [UnityTest]
        public IEnumerator Tween_Completes_Volume([ValueSource(nameof(VolumeCases))] LeanTweenCase<AudioSource, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 1f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }

        [UnityTest]
        public IEnumerator Tween_Completes_Pitch([ValueSource(nameof(PitchCases))] LeanTweenCase<AudioSource, float> tc)
        {
            var comp = tc.CreateComponent(_testGo);
            tc.SetValue(comp, 1f);
            tc.CreateTween(_adapter, comp);
            yield return new WaitForSeconds(Duration + 0.05f);
            Assert.AreEqual(tc.TargetValue, tc.GetValue(comp), Epsilon, tc.Name);
        }
    }
}