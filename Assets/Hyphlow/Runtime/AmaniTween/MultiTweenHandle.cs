using System.Collections.Generic;

namespace AtMycelia.AmaniTween
{
    public sealed class MultiTweenHandle : ITweenHandle
    {
        public MultiTweenHandle(IList<ITweenHandle> handles)
        {
            _handles = handles;
        }

        private readonly IList<ITweenHandle> _handles;

        public void Kill()
        {
            for (int i = 0; i < _handles.Count; i++)
            {
                _handles[i]?.Kill();
            }
        }

        public bool IsPlaying
        {
            get
            {
                for (int i = 0; i < _handles.Count; i++)
                {
                    if (_handles[i] != null && _handles[i].IsPlaying)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public ITweenHandle SetOnComplete(System.Action onComplete)
        {
            _onComplete = onComplete;
            _remaining = _handles.Count;
            for (int i = 0; i < _handles.Count; i++)
            {
                ITweenHandle handle = _handles[i];
                if (handle == null)
                {
                    _remaining--;
                    continue;
                }

                handle.SetOnComplete(OnSingleComplete);
            }

            if (_remaining <= 0)
            {
                _onComplete?.Invoke();
            }

            return this;
        }

        private System.Action _onComplete;

        private void OnSingleComplete()
        {
            _remaining--;
            if (_remaining <= 0)
            {
                _onComplete?.Invoke();
            }
        }

        private int _remaining;
    }
}