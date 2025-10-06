using System;
using UnityEngine;
using Amanita.LeanTweenIntegration;
using Amanita.Tweening;

public class LeanTweenCase<TComponent, TValue> where TComponent : Component
{
    public string Name;
    public Func<AmaniLeanTweenAdapter, TComponent, ITweenHandle> CreateTween;
    public Func<TComponent, TValue> GetValue;
    public Action<TComponent, TValue> SetValue;
    public Func<GameObject, TComponent> CreateComponent;
    public TValue TargetValue;
}