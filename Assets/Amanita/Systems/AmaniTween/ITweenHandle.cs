using System;

namespace Amanita.Tweening
{
    public interface ITweenHandle
    {
        void Kill();
        bool IsPlaying { get; }
        Action OnComplete { get; set; }
    }
}