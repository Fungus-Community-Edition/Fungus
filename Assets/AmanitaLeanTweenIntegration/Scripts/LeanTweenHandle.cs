
using Amanita.Tweening;
using System;

namespace Amanita.LeanTweenIntegration
{
    public class LeanTweenHandle : ITweenHandle
    {
        public static LeanTweenHandle From(LTDescr tween)
        {
            var result = new LeanTweenHandle(tween);
            return result;
        }

        public LeanTweenHandle(LTDescr tween)
        {
            Tween = tween;
        }

        public LTDescr Tween { get; set; }

        // keep an internal stored Action so OnComplete can be read/rewired similarly to DOTweenHandle
        private Action _onComplete = delegate { };

        public virtual void Kill()
        {
            if (Tween == null)
            {
                return;
            }

            try
            {
                LeanTween.cancel(Tween.id);
            }
            catch
            {
                // best-effort cancel
                LeanTween.cancelAll();
            }
        }

        public virtual bool IsPlaying => Tween != null && LeanTween.isTweening(Tween.id);

        public virtual ITweenHandle SetOnComplete(Action arg)
        {
            OnComplete = arg;
            return this;
        }

        public virtual Action OnComplete
        {
            get
            {
                return _onComplete ?? delegate { };
            }
            set
            {
                _onComplete = value ?? delegate { };
                Tween?.setOnComplete(() => _onComplete());
            }
        }
    }

}
