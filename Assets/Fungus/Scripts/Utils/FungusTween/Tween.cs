using System;
using UnityEngine;

namespace Fungus
{
    public class Tween<T> : ITween
    {
        public Tween(object target, string identifier, T startVal,
            T endVal, float duration, Action<T> onTweenUpdate = null)
        {
            Target = target;
            ID = identifier;
            _startValue = startVal;
            _endValue = endVal;
            _duration = duration;
            _onTweenUpdate = onTweenUpdate + delegate { };

            NeoTweenManager.S.AddTween(this);
        }

        public virtual object Target { get; protected set; }
        public virtual string ID { get; protected set; }
        protected T _startValue, _endValue;
        protected float _duration;
        protected Action<T> _onTweenUpdate = delegate { };

        public virtual void Update()
        {
            if (!IsPaused)
            {
                ApplyDelayAsNeeded();

                bool noMoreNeedToDelay = _delayElapsedTime >= DelayTime;
                if (noMoreNeedToDelay)
                {
                    if (IsComplete)
                    {
                        return;
                    }

                    if (IsTargetDestroyed())
                    {
                        FullKill();
                        return;
                    }

                    ElapseTimeAsNeeded();
                    _progress = _elapsedTime / _duration;

                    ApplyInterpolation();
                    _onUpdate?.Invoke();
                    _onTweenUpdate?.Invoke(_currentValue);

                    if (_percentThreshhold >= 0f && _progress >= _percentThreshhold)
                    {
                        _onPercentCompleted?.Invoke();
                        _percentThreshhold = -1;
                    }

                    bool loopFinished = _elapsedTime >= _duration;

                    if (loopFinished)
                    {
                        OnLoopFinished();
                    }
                }
            }
        }

        public virtual bool IsPaused { get; protected set; }

        protected virtual void ApplyDelayAsNeeded()
        {
            if (IgnoreTimeScale)
            {
                _delayElapsedTime += Time.unscaledDeltaTime;
            }
            else
            {
                _delayElapsedTime += Time.deltaTime;
            }
        }

        public virtual bool IgnoreTimeScale { get; protected set; }
        protected float _delayElapsedTime;
        public virtual float DelayTime { get; protected set; }

        public virtual bool IsComplete { get; protected set; }
        protected float _elapsedTime;

        protected virtual void ElapseTimeAsNeeded()
        {
            float timeToElapse;
            if (IgnoreTimeScale)
            {
                timeToElapse = Time.unscaledDeltaTime;
            }
            else
            {
                timeToElapse = Time.deltaTime;
            }

            _elapsedTime += timeToElapse;
        }
        
        protected virtual void ApplyInterpolation()
        {
            if (_reverse)
            {
                _currentValue = Interpolate(_endValue, _startValue, _progress);
            }
            else
            {
                _currentValue = Interpolate(_startValue, _endValue, _progress);
            }
        }

        protected bool _reverse;
        protected float _progress;

        public T Interpolate(T start, T end, float progress)
        {
            object rawResult = null;
            SetRawResultForBasicTypes(start, end, progress, ref rawResult);
            T endResult;

            try
            {
                endResult = (T)rawResult;
            }
            catch
            {
                throw new System.NotImplementedException($"Interpolation for {typeof(T)}s is not implemented.");
            }

            return endResult;
        }

        protected virtual void SetRawResultForBasicTypes(T start, T end, float progress, ref object rawResult)
        {
            if (start is float startFloat && end is float endFloat)
            {
                rawResult = Mathf.Lerp(startFloat, endFloat, progress);
            }

            if (start is Vector3 startVec && end is Vector3 endVec)
            {
                rawResult = Vector3.Lerp(startVec, endVec, progress);
            }

            if (start is Color startColor && end is Color endColor)
            {
                rawResult = Color.Lerp(startColor, endColor, progress);
            }
        }

        protected T _currentValue = default;
        protected Action _onUpdate = delegate { };

        protected float _percentThreshhold = -1f;
        protected Action _onPercentCompleted = delegate { };

        protected virtual void OnLoopFinished()
        {
            _loopsCompleted++;
            _elapsedTime = 0f;

            if (_pingPong)
            {
                _reverse = !_reverse;
            }

            if (_loopsToExecute > 0 && _loopsCompleted >= _loopsToExecute)
            {
                OnCompleteKill();
            }
        }

        protected int _loopsCompleted = 0;
        protected bool _pingPong;

        protected int _loopsToExecute = 1;

        public virtual void FullKill()
        {
            // We want to do this in cases such as there no longer being a target
            // to apply this tween to
            OnCompleteKill();
            WasKilled = true;
            OnComplete = delegate { };
        }

        public virtual bool WasKilled { get; protected set; }
        public virtual Action OnComplete { get; set; } = delegate { };

        public virtual bool IsTargetDestroyed()
        {
            bool whetherItIs = (Target is MonoBehaviour mono && mono == null) ||
                (Target is GameObject go && go == null) ||
                (Target is Delegate del && del.Target == null);

            return whetherItIs;
        }

        public virtual void OnCompleteKill()
        {
            IsComplete = true;
            _onUpdate = delegate { };
            _onTweenUpdate = delegate { };
            _onPercentCompleted = delegate { };
        }

        public virtual void Pause()
        {
            IsPaused = true;
        }

        public virtual void Resume()
        {
            IsPaused = false;
        }

        /// <summary>
        /// If you want this to loop forever, set this to -1
        /// </summary>
        public virtual Tween<T> SetPingPong(int howManyLoopsToGoThrough = 1)
        {
            _loopsToExecute = howManyLoopsToGoThrough;
            _pingPong = true;
            return this;
        }

        public virtual Tween<T> SetOnUpdate(Action newOnUpdate)
        {
            _onUpdate = newOnUpdate;
            return this;
        }

        /// <summary>
        /// The percent has to be on a scale of 0 to 100 (which is the most commonly used irl)
        /// </summary>
        public virtual Tween<T> SetOnPercentCompleted(float newPercentCompleted, Action newOnPercentCompleted)
        {
            return SetOnPercentCompleted01(newPercentCompleted / 100f, newOnPercentCompleted);
        }

        /// <summary>
        /// The percent has to be on a scale of 0 to 1
        /// </summary>
        public virtual Tween<T> SetOnPercentCompleted01(float newPercentCompleted, Action newOnPercentCompleted)
        {
            _percentThreshhold = Mathf.Clamp01(newPercentCompleted);
            _onPercentCompleted = newOnPercentCompleted;
            return this;
        }

        public virtual Tween<T> SetStartDelay(float newDelayTime)
        {
            DelayTime = newDelayTime;
            return this;
        }

        public virtual void TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public virtual Tween<T> SetOnComplete(Action newOnComplete)
        {
            OnComplete = newOnComplete;
            return this;
        }
    }

}