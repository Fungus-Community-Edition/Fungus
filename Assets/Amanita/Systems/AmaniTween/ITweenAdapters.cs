using Amanita.Myceliaudio;
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
    }

    public interface IGraphicTweenAdapter
    {
        ITweenHandle FadeColor(Graphic target, Color endVal, float duration);
        ITweenHandle FadeOpacity(Graphic target, float endVal, float duration);
        ITweenHandle FadeColor(SpriteRenderer target, Color endVal, float duration);
        ITweenHandle FadeOpacityTo(SpriteRenderer target, float endVal, float duration);

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

    public interface IMyceliaudioTweenAdapter
    {
        ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration);
        ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration);
    }

    public interface IGeneralTweenAdapter<T>
    {
        ITweenHandle TweenGeneral(Func<T> getter, Action<T> setter, T endVal, float duration, Action onComplete = null);
    }

    public interface ICameraTweenAdapter
    {
        ITweenHandle ShiftFieldOfViewTo(Camera target, float targetVal, float duration);
        ITweenHandle ShiftOrthographicSizeTo(Camera target, float targetVal, float duration);
        ITweenHandle ShiftBackgroundColorTo(Camera target, Color targetVal, float duration);
    }

    public interface ICinemachineCameraTweenAdapter : ICameraTweenAdapter
    {
        // Not sure what to put here...
    }

    public interface ILightTweenAdapter
    {
        ITweenHandle ShiftIntensityTo(Light target,  float targetVal, float duration);
        ITweenHandle ShiftColorTo(Light target, Color targetVal, float duration);
        ITweenHandle ShiftRangeTo(Light target, float targetVal, float duration);
    }

    public interface ICanvasGroupTweenAdapter
    {
        ITweenHandle FadeOpacity(CanvasGroup target, float alpha, float duration);
    }

    public interface IRectTransformTweenAdapter
    {
        ITweenHandle ShiftAnchoredPositionTo(RectTransform target, Vector2 position, float duration);
        ITweenHandle ShiftSizeDeltaTo(RectTransform target, Vector2 size, float duration);
        ITweenHandle RotateTo(RectTransform target, Quaternion rotation, float duration);
        ITweenHandle ScaleTo(RectTransform target, Vector3 scale, float duration);
    }

    public interface IMaterialTweenAdapter
    {
        ITweenHandle ShiftColorTo(Material target, Color targetVal, float duration);
        ITweenHandle ShiftFloatTo(Material target, string propertyName, float targetVal, float duration);
    }

    public interface IAudioFilterTweenAdapter
    {
        ITweenHandle ShiftLowPassCutoffTo(AudioLowPassFilter target, float targetVal, float duration);
        ITweenHandle ShiftReverbLevelTo(AudioReverbFilter target, float targetVal, float duration);
    }

}