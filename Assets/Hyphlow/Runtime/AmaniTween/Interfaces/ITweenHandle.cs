using System;

namespace AtMycelia.AmaniTween
{
    public interface ITweenHandle
    {
        void Kill();
        bool IsPlaying { get; }
        ITweenHandle SetOnComplete(Action onComplete);
    }
}