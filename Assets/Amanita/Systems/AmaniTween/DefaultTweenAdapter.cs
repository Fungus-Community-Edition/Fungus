using Amanita.Myceliaudio;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Amanita.Tweening
{
    public class DefaultTweenAdapter : ScriptableObject, ITransformTweenAdapter, IGeneralTweenAdapter<Vector2>,
        IGeneralTweenAdapter<Vector3>, IGeneralTweenAdapter<float>, IGeneralTweenAdapter<int>,
        IGraphicTweenAdapter, ICameraTweenAdapter, IAudioSourceTweenAdapter, IMyceliaudioTweenAdapter,
        IMaterialTweenAdapter, IRectTransformTweenAdapter, IAudioFilterTweenAdapter, ILightTweenAdapter
    {

        public ITweenHandle FadeTo(Graphic target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle FadeTo(CanvasGroup target, float endVal, float duration)
        {
            return TweenManager.S.FadeTo(target, endVal, duration);
        }

        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            return TweenManager.S.MoveTo(target, position, duration);
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            return TweenManager.S.RotateTo(target, rotation, duration);
        }

        public ITweenHandle RotateTo(RectTransform target, Quaternion rotation, float duration)
        {
            return TweenManager.S.RotateTo(target, rotation, duration);
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            return TweenManager.S.ScaleTo(target, scale, duration);
        }

        public ITweenHandle ScaleTo(RectTransform target, Vector3 scale, float duration)
        {
            return TweenManager.S.ScaleTo(target, scale, duration);
        }

        public ITweenHandle ShiftAnchoredPositionTo(RectTransform target, Vector2 position, float duration)
        {
            return TweenManager.S.ShiftAnchoredPositionTo(target, position, duration);
        }

        public ITweenHandle ShiftBackgroundColorTo(Camera target, Color targetVal, float duration)
        {
            return TweenManager.S.ShiftBackgroundColorTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration)
        {
            return TweenManager.S.ShiftColorTo(target, endVal, duration);
        }

        public ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration)
        {
            return TweenManager.S.ShiftColorTo(target, endVal, duration);
        }

        public ITweenHandle ShiftColorTo(Material target, Color targetVal, float duration)
        {
            return TweenManager.S.ShiftColorTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftColorTo(Light target, Color targetVal, float duration)
        {
            return TweenManager.S.ShiftColorTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftFieldOfViewTo(Camera target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftFieldOfViewTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            return TweenManager.S.ShiftFillTo(target, endVal, duration);
        }

        public ITweenHandle ShiftFloatTo(Material target, string propertyName, float targetVal, float duration)
        {
            return TweenManager.S.ShiftFloatTo(target, propertyName, targetVal, duration);
        }

        public ITweenHandle ShiftIntensityTo(Light target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftIntensityTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftLowPassCutoffTo(AudioLowPassFilter target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftLowPassCutoffTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftOrthographicSizeTo(Camera target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftOrthographicSizeTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftPitchN33To(AudioSource target, float targVal, float duration)
        {
            return TweenManager.S.ShiftPitchN33To(target, targVal, duration);
        }

        public ITweenHandle ShiftPitchTo(AudioSource target, float targVal, float duration)
        {
            return TweenManager.S.ShiftPitchTo(target, targVal, duration);
        }

        public ITweenHandle ShiftRangeTo(Light target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftRangeTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftReverbLevelTo(AudioReverbFilter target, float targetVal, float duration)
        {
            return TweenManager.S.ShiftReverbLevelTo(target, targetVal, duration);
        }

        public ITweenHandle ShiftSizeDeltaTo(RectTransform target, Vector2 size, float duration)
        {
            return TweenManager.S.ShiftSizeDeltaTo(target, size, duration);
        }

        public ITweenHandle ShiftVolume01To(AudioSource target, float targVal, float duration)
        {
            return TweenManager.S.ShiftVolume01To(target, targVal, duration);
        }

        public ITweenHandle ShiftVolume01To(IAudioTrack track, int targVal, float duration)
        {
            return TweenManager.S.ShiftVolume01To(track, targVal, duration);
        }

        public ITweenHandle ShiftVolumeTo(AudioSource target, float targVal, float duration)
        {
            return TweenManager.S.ShiftVolumeTo(target, targVal, duration);
        }

        public ITweenHandle ShiftVolumeTo(IAudioTrack track, float targVal, float duration)
        {
            return TweenManager.S.ShiftVolumeTo(track, targVal, duration);
        }

        public ITweenHandle TweenGeneral(Func<Vector2> getter, Action<Vector2> setter, Vector2 endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<Vector3> getter, Action<Vector3> setter, Vector3 endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }

        public ITweenHandle TweenGeneral(Func<int> getter, Action<int> setter, int endVal,
            float duration, Action onComplete = null)
        {
            return TweenManager.S.TweenGeneral(getter, setter, endVal, duration, onComplete);
        }
    }

}