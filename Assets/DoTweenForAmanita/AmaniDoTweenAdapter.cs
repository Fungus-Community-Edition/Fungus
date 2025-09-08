using Amanita.Tweening;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

namespace Amanita.ThirdPartyInt.DGDOTween
{

    [CreateAssetMenu(fileName = "NewAmanitaDoTweenAdapter", menuName = "Amanita/DOTween/TweenAdapter")]
    public class AmaniDoTweenAdapter : ScriptableObject, ITransformTweenAdapter,
        IGraphicTweenAdapter, IAudioSourceTweenAdapter, ICameraTweenAdapter,
        ILightTweenAdapter, ICanvasGroupTweenAdapter, IRectTransformTweenAdapter,
        IMaterialTweenAdapter, IAudioFilterTweenAdapter, IGeneralTweenAdapter<float>,
        IGeneralTweenAdapter<int>, IGeneralTweenAdapter<Vector2>, IGeneralTweenAdapter<Vector3>
    {
        [SerializeField] protected Ease _ease = Ease.Linear;

        public virtual Ease Ease
        {
            get => _ease;
            set => _ease = value;
        }

        #region Transform
        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            Tween tween = target.DOMove(position, duration).SetEase(_ease);
            DOTweenHandle tweenHandle = new DOTweenHandle(tween);
            return tweenHandle;
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            Tween tween = target.DORotateQuaternion(rotation, duration).SetEase(_ease);
            DOTweenHandle handle = new DOTweenHandle(tween);
            return handle;
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            Tween tween = target.DOScale(scale, duration).SetEase(_ease);
            DOTweenHandle dOTweenHandle = new DOTweenHandle(tween);
            return dOTweenHandle;
        }
        #endregion

        #region Graphic
        public ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration)
        {
            Tween tween = target.DOColor(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration)
        {
            Tween tween = target.DOColor(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle FadeTo(Graphic target, float endVal, float duration)
        {
            Tween tween = target.DOFade(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration)
        {
            Tween tween = target.DOFade(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }


        #endregion

        #region AudioSource

        public ITweenHandle ShiftVolumeTo(AudioSource target, float targVal, float duration)
        {
            Tween tween = target.DOFade(targVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle ShiftVolume02To(AudioSource target, float targVal, float duration)
        {
            return ShiftVolumeTo(target, targVal / 100f, duration);
        }

        public ITweenHandle ShiftPitchTo(AudioSource target, float targVal, float duration)
        {
            Tween tween = target.DOPitch(targVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle ShiftPitch02To(AudioSource target, float targVal, float duration)
        {
            return ShiftPitchTo(target, targVal / 100f, duration);
        }

        #endregion

        #region Camera
        public ITweenHandle ShiftFieldOfViewTo(Camera target, float targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.fieldOfView, v => target.fieldOfView = v, targetVal, duration)
                                 .SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftOrthographicSizeTo(Camera target, float targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.orthographicSize, v => target.orthographicSize = v, targetVal, duration)
                                 .SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftBackgroundColorTo(Camera target, Color targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.backgroundColor, v => target.backgroundColor = v, targetVal, duration)
                                 .SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        #region Light
        public ITweenHandle ShiftIntensityTo(Light target, float targetVal, float duration)
        {
            Tween tween = target.DOIntensity(targetVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftColorTo(Light target, Color targetVal, float duration)
        {
            Tween tween = target.DOColor(targetVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftRangeTo(Light target, float targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.range,
                newRangeVal => target.range = newRangeVal,
                targetVal, duration)
                                 .SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        #region CanvasGroup
        public ITweenHandle FadeTo(CanvasGroup target, float endVal, float duration)
        {
            Tween tween = target.DOFade(endVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        #region RectTransform
        public ITweenHandle ShiftAnchoredPositionTo(RectTransform target, Vector2 position, float duration)
        {
            Tween tween = target.DOAnchorPos(position, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftSizeDeltaTo(RectTransform target, Vector2 size, float duration)
        {
            Tween tween = target.DOSizeDelta(size, duration).SetEase(_ease); ;
            return new DOTweenHandle(tween);
        }

        public ITweenHandle RotateTo(RectTransform target, Quaternion rotation, float duration)
        {
            Tween tween = target.DORotateQuaternion(rotation, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ScaleTo(RectTransform target, Vector3 scale, float duration)
        {
            Tween tween = target.DOScale(scale, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        #region Material
        public ITweenHandle ShiftColorTo(Material target, Color targetVal, float duration)
        {
            Tween tween = target.DOColor(targetVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftFloatTo(Material target, string propertyName, float targetVal, float duration)
        {
            Tween tween = target.DOFloat(targetVal, propertyName, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        #region Audio Filters
        public ITweenHandle ShiftLowPassCutoffTo(AudioLowPassFilter target, float targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.cutoffFrequency,
                newVal => target.cutoffFrequency = newVal,
                targetVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftReverbLevelTo(AudioReverbFilter target, float targetVal, float duration)
        {
            Tween tween = DOTween.To(() => target.reverbLevel,
                newVal => target.reverbLevel = newVal,
                targetVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle ShiftFillTo(Image target, float endVal, float duration)
        {
            Tween tween = target.DOFillAmount(endVal, duration).SetEase(_ease);
            return new DOTweenHandle(tween);
        }
        #endregion

        public ITweenHandle TweenGeneral(Func<float> getter, Action<float> setter, float endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Tween tween = DOTween.To(() => { return getter(); }, (val) => { setter(val); }, endVal, duration)
                .OnComplete(() => { onComplete(); })
                .SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle TweenGeneral(Func<int> getter, Action<int> setter, int endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Tween tween = DOTween.To(() => { return getter(); }, (val) => { setter(val); }, endVal, duration)
                .OnComplete(() => { onComplete(); })
                .SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle TweenGeneral(Func<Vector2> getter, Action<Vector2> setter, Vector2 endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Tween tween = DOTween.To(() => { return getter(); }, (val) => { setter(val); }, endVal, duration)
                .OnComplete(() => { onComplete(); })
                .SetEase(_ease);
            return new DOTweenHandle(tween);
        }

        public ITweenHandle TweenGeneral(Func<Vector3> getter, Action<Vector3> setter, Vector3 endVal,
            float duration, Action onComplete = null)
        {
            onComplete ??= delegate { };
            Tween tween = DOTween.To(() => { return getter(); }, (val) => { setter(val); }, endVal, duration)
                .OnComplete(() => { onComplete(); })
                .SetEase(_ease);
            return new DOTweenHandle(tween);
        }


    }


}

