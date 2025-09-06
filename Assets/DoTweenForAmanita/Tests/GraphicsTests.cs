using Amanita.ThirdPartyInt.DGDOTween;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class GraphicsTests : DoTweenAdapterTests
{
    // --- Case definitions ---
    private static readonly TweenCase<Graphic, Color> ShiftGraphicCase = new TweenCase<Graphic, Color>
    {
        Name = "ShiftColorTo_Graphic",
        CreateTween = (adapter, graphic) => adapter.ShiftColorTo(graphic, Color.red, Duration),
        GetValue = graphic => graphic.color,
        SetValue = (graphic, color) => graphic.color = color,
        CreateComponent = go => go.AddComponent<Image>(),
        TargetValue = Color.red
    };

    private static readonly TweenCase<SpriteRenderer, Color> ShiftSpriteCase = new TweenCase<SpriteRenderer, Color>
    {
        Name = "ShiftColorTo_SpriteRenderer",
        CreateTween = (adapter, spriteRenderer) => adapter.ShiftColorTo(spriteRenderer, Color.green, Duration),
        GetValue = spriteRenderer => spriteRenderer.color,
        SetValue = (spriteRenderer, color) => spriteRenderer.color = color,
        CreateComponent = go => go.AddComponent<SpriteRenderer>(),
        TargetValue = Color.green
    };

    private static readonly TweenCase<Graphic, Color> FadeGraphicCase = new TweenCase<Graphic, Color>
    {
        Name = "FadeTo_Graphic",
        CreateTween = (adapter, graphic) => adapter.FadeTo(graphic, 0.5f, Duration),
        GetValue = graphic => graphic.color,
        SetValue = (graphic, color) => graphic.color = color,
        CreateComponent = go => go.AddComponent<Image>(),
        TargetValue = new Color(1f, 1f, 1f, 0.5f)
    };

    private static readonly TweenCase<SpriteRenderer, Color> FadeSpriteCase = new TweenCase<SpriteRenderer, Color>
    {
        Name = "FadeTo_SpriteRenderer",
        CreateTween = (adapter, spriteRenderer) => adapter.FadeTo(spriteRenderer, 0.25f, Duration),
        GetValue = spriteRenderer => spriteRenderer.color,
        SetValue = (spriteRenderer, color) => spriteRenderer.color = color,
        CreateComponent = go => go.AddComponent<SpriteRenderer>(),
        TargetValue = new Color(1f, 1f, 1f, 0.25f)
    };

    // --- Sources ---
    private static readonly object[] GraphicCases = { ShiftGraphicCase, FadeGraphicCase };
    private static readonly object[] SpriteCases = { ShiftSpriteCase, FadeSpriteCase };

    // --- Non-yield tests ---
    [TestCaseSource(nameof(GraphicCases))]
    public void Handle_IsValid_Graphic(TweenCase<Graphic, Color> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    [TestCaseSource(nameof(SpriteCases))]
    public void Handle_IsValid_Sprite(TweenCase<SpriteRenderer, Color> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.IsInstanceOf<DOTweenHandle>(handle);
        Assert.IsNotNull(((DOTweenHandle)handle).Tween);
    }

    [TestCaseSource(nameof(GraphicCases))]
    public void Kill_DoesNotThrow_Graphic(TweenCase<Graphic, Color> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    [TestCaseSource(nameof(SpriteCases))]
    public void Kill_DoesNotThrow_Sprite(TweenCase<SpriteRenderer, Color> tCase)
    {
        var comp = tCase.CreateComponent(_testGo);
        var handle = tCase.CreateTween(_adapter, comp);
        Assert.DoesNotThrow(() => handle.Kill());
        Assert.IsFalse(handle.IsPlaying);
    }

    // --- Yield tests ---
    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue_Graphic(
        [ValueSource(nameof(GraphicCases))] TweenCase<Graphic, Color> tCase)
    {
        yield return RunTweenCase(tCase, Color.white);
    }

    [UnityTest]
    public IEnumerator Tween_CompletesWithExpectedValue_Sprite(
        [ValueSource(nameof(SpriteCases))] TweenCase<SpriteRenderer, Color> tCase)
    {
        yield return RunTweenCase(tCase, Color.white);
    }

    // --- Shared runner ---
    private IEnumerator RunTweenCase<T>(TweenCase<T, Color> tCase, Color startValue) where T : Component
    {
        var comp = tCase.CreateComponent(_testGo);
        tCase.SetValue(comp, startValue);

        tCase.CreateTween(_adapter, comp);

        yield return new WaitForSeconds(Duration + 0.05f);

        var actual = tCase.GetValue(comp);
        Assert.AreEqual(tCase.TargetValue.r, actual.r, Epsilon, $"{tCase.Name} - R");
        Assert.AreEqual(tCase.TargetValue.g, actual.g, Epsilon, $"{tCase.Name} - G");
        Assert.AreEqual(tCase.TargetValue.b, actual.b, Epsilon, $"{tCase.Name} - B");
        Assert.AreEqual(tCase.TargetValue.a, actual.a, Epsilon, $"{tCase.Name} - A");
    }
}