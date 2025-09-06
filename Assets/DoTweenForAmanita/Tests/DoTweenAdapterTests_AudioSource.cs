using Amanita.ThirdPartyInt.DGDOTween;
using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObj = UnityEngine.Object;

public class DoTweenAdapterTests_AudioSource
{
    private GameObject _go;
    private AmaniDoTweenAdapter _adapter;

    private const float Duration = 1f;
    private const float Epsilon = 1e-3f;

    // --- Case definitions ---
    private static readonly TweenCase<AudioSource, float> ShiftVolumeCase = new TweenCase<AudioSource, float>
    {
        Name = "ShiftVolumeTo",
        CreateTween = (adapter, src) => adapter.ShiftVolumeTo(src, 0.5f, Duration),
        GetValue = src => src.volume,
        SetValue = (src, v) => src.volume = v,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 0.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftVolume02Case = new TweenCase<AudioSource, float>
    {
        Name = "ShiftVolume02To",
        CreateTween = (adapter, src) => adapter.ShiftVolume02To(src, 50f, Duration), // 50% → 0.5f
        GetValue = src => src.volume,
        SetValue = (src, v) => src.volume = v,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 0.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftPitchCase = new TweenCase<AudioSource, float>
    {
        Name = "ShiftPitchTo",
        CreateTween = (adapter, src) => adapter.ShiftPitchTo(src, 1.5f, Duration),
        GetValue = src => src.pitch,
        SetValue = (src, v) => src.pitch = v,
        CreateComponent = go => go.AddComponent<AudioSource>(),
        TargetValue = 1.5f
    };

    private static readonly TweenCase<AudioSource, float> ShiftPitch02Case = new TweenCase<AudioSource, float>
    {
        Name = "ShiftPitch02To",
        CreateTween = (adapter, src) => adapter.ShiftPitch02To(src, 150f, Duration), // 150% → 1.5f
        GetValue = src => src.pitch,
        SetValue = (src, v) => src.pitch = v,
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

    [SetUp]
    public void SetUp()
    {
        DOTween.KillAll(false);
        _go = new GameObject("AudioTweenTestGO");
        _adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();
    }

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll(false);
        if (_go) UnityObj.DestroyImmediate(_go);
        if (_adapter) UnityObj.DestroyImmediate(_adapter);
    }

    // --- Non-yield tests ---
    [TestCaseSource(nameof(AudioCases))]
    public void Handle_IsValid(TweenCase<AudioSource, float> tc)
    {
        var comp = tc.CreateComponent(_go);
        var handle = tc.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    [TestCaseSource(nameof(AudioCases))]
    public void Kill_DoesNotThrow(TweenCase<AudioSource, float> tc)
    {
        var comp = tc.CreateComponent(_go);
        var handle = tc.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    // --- Yield tests ---
    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue(
        [ValueSource(nameof(AudioCases))] TweenCase<AudioSource, float> tc)
    {
        var comp = tc.CreateComponent(_go);
        tc.SetValue(comp, 0f); // start from zero for volume/pitch

        tc.CreateTween(_adapter, comp);

        yield return new WaitForSeconds(Duration + 0.05f);

        var actual = tc.GetValue(comp);
        Assert.AreEqual(tc.TargetValue, actual, Epsilon, tc.Name);
    }

    // --- Edge-case tests for 02 methods ---
    [UnityTest]
    public IEnumerator ShiftVolume02To_ZeroPercent_YieldsZero()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, 0f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0f, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftVolume02To_HundredPercent_YieldsOne()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, 100f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(1f, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_ZeroPercent_YieldsZero()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 0f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0f, src.pitch, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_TwoHundredPercent_YieldsTwo()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 200f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(2f, src.pitch, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftVolume02To_NegativePercent_ScalesToZero()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftVolume02To(src, -50f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(0, src.volume, Epsilon);
    }

    [UnityTest]
    public IEnumerator ShiftPitch02To_AboveTwoHundredPercent_ScalesAboveTwo()
    {
        var src = _go.AddComponent<AudioSource>();
        var handle = _adapter.ShiftPitch02To(src, 300f, Duration);
        yield return new WaitForSeconds(Duration + 0.05f);
        Assert.AreEqual(3f, src.pitch, Epsilon);
    }
}