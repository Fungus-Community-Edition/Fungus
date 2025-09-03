using System;
using UnityEngine;
using UnityEngine.UI;

namespace Amanita.Tweening
{
    public interface ITransformTweenAdapter
    {
        ITweenHandle MoveTo(Transform target, Vector3 position, float duration);
        ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration);
        ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration);
        // …plus any easing, delay, or chaining methods you want
    }

    public interface IGraphicTweenAdapter
    {
        ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration);
        ITweenHandle FadeTo(Graphic target, float endVal, float duration);
        ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration);
        ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration);
    }

}