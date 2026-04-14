using System;

namespace AtMycelia.Hyphlow.Tweening
{
    public interface ITweenHandle
    {
        void Kill();
        bool IsPlaying { get; }
        ITweenHandle SetOnComplete(Action onComplete);
    }
}