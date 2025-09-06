using Amanita.Tweening;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

namespace Amanita.ThirdPartyInt.DGDOTween
{
    public class AmaniDoTweenAdapter : ScriptableObject, ITransformTweenAdapter, IGraphicTweenAdapter,
        IAudioSourceTweenAdapter
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

    }

    
}

