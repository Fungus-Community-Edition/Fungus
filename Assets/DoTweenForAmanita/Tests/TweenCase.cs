using Amanita.ThirdPartyInt.DGDOTween;
using Amanita.Tweening;
using System;
using UnityEngine;

public class TweenCase<T> where T : Component
{
    public string Name;
    public Func<AmaniDoTweenAdapter, T, ITweenHandle> CreateTween;
    public Func<T, Color> GetValue;
    public Action<T, Color> SetValue;
    public Func<GameObject, T> CreateComponent; // NEW: explicit factory
    public Color? TargetValue;
    public float? TargetAlpha;
}
