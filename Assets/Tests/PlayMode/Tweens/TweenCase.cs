using AtMycelia.AmaniTween;
using System;
using UnityEngine;

public class TweenCase<TComponent, TValue> where TComponent : Component
{
    public string Name;
    public Func<DefaultTweenAdapter, TComponent, ITweenHandle> CreateTween;
    public Func<TComponent, TValue> GetValue;
    public Action<TComponent, TValue> SetValue;
    public Func<GameObject, TComponent> CreateComponent;
    public TValue TargetValue;
}
