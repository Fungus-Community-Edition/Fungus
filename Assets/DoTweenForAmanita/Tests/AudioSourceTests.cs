using Amanita.ThirdPartyInt.DGDOTween;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class AudioSourceTests : DoTweenAdapterTests
{
    private static readonly TweenCase<AudioSource, float> ShiftVolumeCase = new TweenCase<AudioSource, float>
    {
        Name = "ShiftVolumeTo",
        CreateTween = (adapter, src) => adapter.ShiftVolumeTo(src, 0.5f, Duration),
        GetValue = src => src.volume,
        SetValue = (src, val) => src.volume = val,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 0.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftVolume02Case = new TweenCase<AudioSource, float>
    {
        Name = "ShiftVolume02To",
        CreateTween = (adapter, src) => adapter.ShiftVolume02To(src, 50f, Duration), // 50% → 0.5f
        GetValue = src => src.volume,
        SetValue = (src, val) => src.volume = val,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 0.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftPitchCase = new TweenCase<AudioSource, float>
    {
        Name = "ShiftPitchTo",
        CreateTween = (adapter, src) => adapter.ShiftPitchTo(src, 1.5f, Duration),
        GetValue = src => src.pitch,
        SetValue = (src, val) => src.pitch = val,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 1.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftPitch02Case = new TweenCase<AudioSource, float>
    {
        Name = "ShiftPitch02To",
        CreateTween = (adapter, src) => adapter.ShiftPitch02To(src, 150f, Duration), // 150% → 1.5f
        GetValue = src => src.pitch,
        SetValue = (src, val) => src.pitch = val,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 1.5f
    };

    // --- Sources ---
    private static readonly object[] AudioCases =
    {
        ShiftVolumeCase,
        ShiftVolume02Case,
        ShiftPitchCase,
        ShiftPitch02Case
    };


    // --- Non-yield tests ---
    [TestCaseSource(nameof(AudioCases))]
    public void Handle_IsValid(TweenCase<AudioSource, float> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    [TestCaseSource(nameof(AudioCases))]
    public void Kill_DoesNotThrow(TweenCase<AudioSource, float> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    // --- Yield tests ---
    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue(
        [ValueSource(nameof(AudioCases))] TweenCase<AudioSource, float> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        tCase.SetValue(comp, 0f); // start from zero for volume/pitch

        tCase.CreateTween(_adapter, comp);

        yield return new WaitForSeconds(Duration + 0.05f);

        var actual = tCase.GetValue(comp);
        Assert.AreEqual(tCase.TargetValue, actual, Epsilon, tCase.Name);
    }

    // --- Edge-case tests for 02 methods ---
    [UnityTest]
    public IEnumerator ShiftVolume02To_ZeroPercent_YieldsZero()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, 0f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0f, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftVolume02To_HundredPercent_YieldsOne()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, 100f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(1f, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_ZeroPercent_YieldsZero()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 0f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0f, src.pitch, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_TwoHundredPercent_YieldsTwo()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 200f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(2f, src.pitch, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftVolume02To_NegativePercent_ScalesToZero()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, -50f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_AboveTwoHundredPercent_ScalesAboveTwo()
    {
        var src = _testGo.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 300f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(3f, src.pitch, Epsilon);
    }


    // --- Edge-case data sources ---
    private static readonly object[] Volume02EdgeCases =
{
    new object[] { 0f, 0f },     // 0% → 0.0
    new object[] { 100f, 1f },   // 100% → 1.0
    new object[] { -50f, 0f }    // negative → clamp to 0.0
};

    private static readonly object[] Pitch02EdgeCases =
    {
    new object[] { 0f, 0f },     // 0% → 0.0
    new object[] { 200f, 2f },   // 200% → 2.0
    new object[] { 300f, 3f }    // >200% → scale above 2.0
};

    // --- Parameterized UnityTests ---
    [UnityTest]
    public IEnumerator ShiftVolume02To_EdgeCases(
    [ValueSource(nameof(Volume02EdgeCases))] object[] caseData)
    {
        float inputPercent = (float)caseData[0];
        float expected = (float)caseData[1];

        var src = _testGo.AddComponent<AudioSource>();
        _adapter.ShiftVolume02To(src, inputPercent, Duration);

        yield return new WaitForSeconds(Duration + 0.05f);

        Assert.AreEqual(expected, src.volume, Epsilon,
            $"Volume02To({inputPercent}%) should end at {expected}");
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_EdgeCases(
        [ValueSource(nameof(Pitch02EdgeCases))] object[] caseData)
    {
        float inputPercent = (float)caseData[0];
        float expected = (float)caseData[1];

        var src = _testGo.AddComponent<AudioSource>();
        _adapter.ShiftPitch02To(src, inputPercent, Duration);

        yield return new WaitForSeconds(Duration + 0.05f);

        Assert.AreEqual(expected, src.pitch, Epsilon,
            $"Pitch02To({inputPercent}%) should end at {expected}");
    }
}