using Amanita.Tweening;
using System;
using DG.Tweening;

namespace Amanita.ThirdPartyInt.DGDOTween
{
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