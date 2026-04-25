using System;
using UnityEngine;
using UnityEngine.UI;

namespace AtMycelia.AmaniTween
{
    public interface ITransformTweenAdapter
    {
        ITweenHandle MoveTo(Transform target, Vector3 position, float duration);
        ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration);
        ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration);
        ITweenHandle RotateLocalTo(Transform target, Quaternion rotation, float duration);
    }

    public interface IGraphicTweenAdapter
    {
        ITweenHandle FadeColor(Graphic target, Color endVal, float duration);
        ITweenHandle FadeOpacity(Graphic target, float endVal, float duration);
        ITweenHandle FadeColor(SpriteRenderer target, Color endVal, float duration);
        ITweenHandle FadeOpacity(SpriteRenderer target, float endVal, float duration);

        ITweenHandle FadeOpacity(CanvasGroup target, float endVal, float duration);
        ITweenHandle ShiftFillTo(Image target, float endVal, float duration);
    }

    public interface IAudioSourceTweenAdapter
    {
        /// <summary>
        /// On a scale of 0 for silent to 100 for max.
        ITweenHandle FadeVolume(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of 0 for silent to 1 for max.
        /// </summary>
        ITweenHandle FadeVolume01(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of -300 for min to 300 for max. Note that the default pitch here is 100.
        /// </summary>
        ITweenHandle FadePitch(AudioSource target, float targVal, float duration);

        /// <summary>
        /// On a scale of -3 for min to 3 for max. Note that the default pitch here is 1.
        /// </summary>
        ITweenHandle FadePitchN33(AudioSource target, float targVal, float duration);
    }

    public interface IGeneralTweenAdapter<T>
    {
        ITweenHandle TweenGeneral(Func<T> getter, Action<T> setter, T endVal, float duration, Action onComplete = null);
    }

    public interface ICameraTweenAdapter
    {
        ITweenHandle TweenFOV(Camera target, float targetVal, float duration);
        ITweenHandle TweenOrthoSize(Camera target, float targetVal, float duration);
        ITweenHandle FadeBackgroundColor(Camera target, Color targetVal, float duration);
    }

    public interface ICinemachineCameraTweenAdapter : ICameraTweenAdapter
    {
        // Not sure what to put here...
    }

    public interface ILightTweenAdapter
    {
        ITweenHandle TweenIntensity(Light target,  float targetVal, float duration);
        ITweenHandle FadeColor(Light target, Color targetVal, float duration);
        ITweenHandle FadeColor(Light target, float targetVal, float duration);
    }

    public interface ICanvasGroupTweenAdapter
    {
        ITweenHandle FadeOpacity(CanvasGroup target, float alpha, float duration);
    }

    public interface IRectTransformTweenAdapter : ITransformTweenAdapter
    {
        ITweenHandle TweenAnchoredPosition(RectTransform target, Vector2 endPos, float duration);
        ITweenHandle TweenSizeDelta(RectTransform target, Vector2 endSize, float duration);
    }

    public interface IMaterialTweenAdapter
    {

        ITweenHandle FadeColor(GameObject owner, Material target, Color targetVal, float duration);
        ITweenHandle TweenFloat(GameObject owner, Material target, string propertyName, float targetVal, float duration);

        /// <summary>
        /// Applies to all GameObjects using this material.
        /// </summary>
        ITweenHandle FadeColor(Material target, Color targetVal, float duration);

        /// <summary>
        /// Applies to all GameObjects using this material.
        /// </summary>
        ITweenHandle TweenFloat(Material target, string propertyName, float targetVal, float duration);
    }

    public interface IAudioFilterTweenAdapter
    {
        ITweenHandle FadeLowPassCutoff(AudioLowPassFilter target, float targetVal, float duration);
        ITweenHandle FadeReverbLevel(AudioReverbFilter target, float targetVal, float duration);
    }

}