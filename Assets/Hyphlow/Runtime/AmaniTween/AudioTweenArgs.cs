using UnityEngine;
using UnityEngine.Events;

namespace AtMycelia.AmaniTween
{
    public class AudioTweenArgs : TweenArgs<AudioSource, float>
    {
        public UnityAction<AudioTweenArgs> OnComplete = delegate { };

        public virtual float BaseValNormalized
        {
            get { return BaseValue / 100f; }
        }
        public virtual float TargValNormalized
        {
            get { return TargetValue / 100f; }
        }

        public override void Reset()
        {
            base.Reset();
            Target = null;
            TargetValue = 0;
            OnComplete = delegate { };
        }
    }
}