using Amanita.ThirdPartyInt.DGDOTween;
using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityObj = UnityEngine.Object;

public class DoTweenAdapterTests_Graphics
{
    [SetUp]
    public void SetUp()
    {
        DOTween.KillAll(false);
        _go = new GameObject("TweenTestGO");
        _adapter = ScriptableObject.CreateInstance<AmaniDoTweenAdapter>();
    }

    protected GameObject _go;
    protected AmaniDoTweenAdapter _adapter;

    [TearDown]
    public void TearDown()
    {
        DOTween.KillAll(false);
        if (_go) UnityObj.DestroyImmediate(_go);
        if (_adapter) UnityObj.DestroyImmediate(_adapter);
    }

    // --- Non-yield tests ---
    [TestCaseSource(nameof(GraphicCases))]
    public void Handle_IsValid_Graphic(TweenCase<Graphic> tc)
    {
        var comp = tc.CreateComponent(_go);
        var handle = tc.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    [TestCaseSource(nameof(SpriteCases))]
    public void Handle_IsValid_Sprite(TweenCase<SpriteRenderer> tc)
    {
        var comp = tc.CreateComponent(_go);
        var handle = tc.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    protected static readonly object[] AllCases =
    {
        ShiftGraphicCase,
        ShiftSpriteCase,
        FadeGraphicCase,
        FadeSpriteCase
    };

    protected static readonly TweenCase<Graphic> ShiftGraphicCase = new TweenCase<Graphic>
    {
        Name = "ShiftColorTo_Graphic",
        CreateTween = (adapter, graphic) => adapter.ShiftColorTo(graphic, Color.red, Duration),
        GetValue = graphic => graphic.color,
        SetValue = (graphic, color) => graphic.color = color,
        CreateComponent = go => go.AddComponent<Image>(), // concrete subclass
        TargetValue = Color.red
    };

    protected const float Duration = 1f;

    protected static readonly TweenCase<SpriteRenderer> ShiftSpriteCase = new TweenCase<SpriteRenderer>
    {
        Name = "ShiftColorTo_SpriteRenderer",
        CreateTween = (adapter, spriteRenderer) => adapter.ShiftColorTo(spriteRenderer, Color.green, Duration),
        GetValue = spriteRenderer => spriteRenderer.color,
        SetValue = (spriteRenderer, color) => spriteRenderer.color = color,
        CreateComponent = go => go.AddComponent<SpriteRenderer>(),
        TargetValue = Color.green
    };

    protected static readonly TweenCase<Graphic> FadeGraphicCase = new TweenCase<Graphic>
    {
        Name = "FadeTo_Graphic",
        CreateTween = (adapter, graphic) => adapter.FadeTo(graphic, 0.5f, Duration),
        GetValue = graphic => graphic.color,
        SetValue = (graphic, color) => graphic.color = color,
        CreateComponent = go => go.AddComponent<Image>(),
        TargetAlpha = 0.5f
    };

    protected static readonly TweenCase<SpriteRenderer> FadeSpriteCase = new TweenCase<SpriteRenderer>
    {
        Name = "FadeTo_SpriteRenderer",
        CreateTween = (adapter, spriteRenderer) => adapter.FadeTo(spriteRenderer, 0.25f, Duration),
        GetValue = spriteRenderer => spriteRenderer.color,
        SetValue = (spriteRenderer, color) => spriteRenderer.color = color,
        CreateComponent = go => go.AddComponent<SpriteRenderer>(),
        TargetAlpha = 0.25f
    };

    [TestCaseSource(nameof(GraphicCases))]
    public void Kill_DoesNotThrow_Graphic(TweenCase<Graphic> tCase)
    {
        var comp = tCase.CreateComponent(_go);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    [TestCaseSource(nameof(SpriteCases))]
    public void Kill_DoesNotThrow_Sprite(TweenCase<SpriteRenderer> tCase)
    {
        var comp = tCase.CreateComponent(_go);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    // --- Yield tests ---
    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue_Graphic(
        [ValueSource(nameof(GraphicCases))] TweenCase<Graphic> tCase)
    {
        yield return RunTweenCase(tCase);
    }

    protected static readonly object[] GraphicCases = { ShiftGraphicCase, FadeGraphicCase };

    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue_Sprite(
        [ValueSource(nameof(SpriteCases))] TweenCase<SpriteRenderer> tCase)
    {
        yield return RunTweenCase(tCase);
    }

    protected static readonly object[] SpriteCases = { ShiftSpriteCase, FadeSpriteCase };

    // --- Shared runner ---
    protected IEnumerator RunTweenCase<T>(TweenCase<T> tCase) where T : Component
    {
        var comp = tCase.CreateComponent(_go);
        tCase.SetValue(comp, Color.white);

        tCase.CreateTween(_adapter, comp);

        yield return new WaitForSeconds(Duration + 0.05f);

        var actual = tCase.GetValue(comp);

        if (tCase.TargetAlpha.HasValue)
        {
            Assert.AreEqual(tCase.TargetAlpha.Value, actual.a, Epsilon, tCase.Name);
        }

        else if (tCase.TargetValue.HasValue)
        {
            Assert.AreEqual(tCase.TargetValue.Value.r, actual.r, Epsilon, tCase.Name);
            Assert.AreEqual(tCase.TargetValue.Value.g, actual.g, Epsilon, tCase.Name);
            Assert.AreEqual(tCase.TargetValue.Value.b, actual.b, Epsilon, tCase.Name);
        }
    }

    protected const float Epsilon = 1e-3f;
}