using System;

namespace AtMycelia.AmaniTween
{
    public interface ITween 
    {
        void Update();
        void OnCompleteKill();
        void FullKill();
        bool IsTargetDestroyed();
        void Pause();
        void Resume();
        object Target { get; }
        bool IsComplete { get; }
        bool WasKilled { get; }
        bool IsPaused { get; }
        bool IgnoreTimeScale { get; }
        string ID { get; }
        float DelayTime { get; }
        Action OnComplete { get; set; }
    }
}