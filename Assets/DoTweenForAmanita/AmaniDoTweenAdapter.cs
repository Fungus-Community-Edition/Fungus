using Amanita.Tweening;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

namespace Amanita.ThirdPartyInt.DGDOTween
{
    public class AmaniDoTweenAdapter : ScriptableObject, ITransformTweenAdapter, IGraphicTweenAdapter
    {
        [SerializeField] protected Ease _ease = Ease.Linear;
        public virtual Ease Ease
        {
            get => _ease;
            set => _ease = value;
        }

        public ITweenHandle MoveTo(Transform target, Vector3 position, float duration)
        {
            Tween tween = target.DOMove(position, duration)
                .SetEase(_ease);
            DOTweenHandle tweenHandle = new DOTweenHandle(tween);
            return tweenHandle;
        }

        public ITweenHandle RotateTo(Transform target, Quaternion rotation, float duration)
        {
            Tween tween = target.DORotateQuaternion(rotation, duration)
                .SetEase(_ease);
            DOTweenHandle handle = new DOTweenHandle(tween);
            return handle;
        }

        public ITweenHandle ScaleTo(Transform target, Vector3 scale, float duration)
        {
            Tween tween = target.DOScale(scale, duration).SetEase(_ease);
            DOTweenHandle dOTweenHandle = new DOTweenHandle(tween);
            return dOTweenHandle;
        }

        public ITweenHandle ShiftColorTo(Graphic target, Color endVal, float duration)
        {
            Tween tween = target.DOColor(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle ShiftColorTo(SpriteRenderer target, Color endVal, float duration)
        {
            Tween tween = target.DOColor(endVal,duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle FadeTo(Graphic target, float endVal, float duration)
        {
            Tween tween = target.DOFade(endVal,duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }

        public ITweenHandle FadeTo(SpriteRenderer target, float endVal, float duration)
        {
            Tween tween = target.DOFade(endVal, duration).SetEase(_ease);
            DOTweenHandle result = new DOTweenHandle(tween);
            return result;
        }
    }

    public class DOTweenHandle : ITweenHandle
    {
        public static DOTweenHandle From(Tween tween)
        {
            var result = new DOTweenHandle(tween);
            return result;
        }

        public DOTweenHandle(Tween tween)
        {
            Tween = tween;
        }

        public virtual void Kill()
        {
            Tween?.Kill();
        }

        public virtual Tween Tween { get; set; }

        public virtual bool IsPlaying => Tween != null && Tween.IsPlaying();

        public virtual Action OnComplete
        {
            get
            {
                Action result = delegate { };
                if (Tween != null)
                {
                    result = () => Tween.onComplete()
                    ;
                }

                return result;
            }
            set
            {
                Tween?.OnComplete(() => value());
            }
        }
    }
}

