using System;

namespace AtMycelia.AmaniTween
{
    public class DefaultTweenHandle : ITweenHandle
    {
        public static DefaultTweenHandle From(ITween tween)
        {
            var result = new DefaultTweenHandle(tween);
            return result;
        }

        public DefaultTweenHandle(ITween tween)
        {
            Tween = tween;
        }

        public virtual void Kill()
        {
            Tween?.FullKill();
        }

        public virtual ITween Tween { get; set; }

        public virtual bool IsPlaying => Tween != null && !Tween.WasKilled;

        public virtual ITweenHandle SetOnComplete(Action arg)
        {
            OnComplete = arg;
            return this;
        }

        public virtual Action OnComplete
        {
            get
            {
                Action result = delegate { };
                if (Tween != null)
                {
                    result = () => Tween.OnComplete();
                }

                return result;
            }
            set
            {
                if (Tween != null)
                {
                    Tween.OnComplete = value;
                }
            }
        }
    }

}