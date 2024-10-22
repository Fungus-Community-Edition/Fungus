using UnityEngine;
using UnityEngine.Events;

namespace Fungus
{
    public class AudioTweenArgs : TweenArgs<AudioSource, float>
    {
        public UnityAction<AudioTweenArgs> OnComplete = delegate { };

        public override void Reset()
        {
            base.Reset();
            Target = null;
            TargetValue = 0;
            OnComplete = delegate { };
        }
    }
}