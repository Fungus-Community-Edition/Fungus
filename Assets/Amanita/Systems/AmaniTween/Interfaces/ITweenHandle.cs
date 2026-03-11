using System;

namespace AtMycelia.Amanita.Tweening
{
    public interface ITweenHandle
    {
        void Kill();
        bool IsPlaying { get; }
        ITweenHandle SetOnComplete(Action onComplete);
    }
}