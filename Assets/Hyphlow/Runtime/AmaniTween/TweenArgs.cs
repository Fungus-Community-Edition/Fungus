using UnityEngine.Events;

namespace AtMycelia.AmaniTween
{
    public class TweenArgs : System.EventArgs
    {
        public virtual System.Object BaseValue { get; set; }
        public virtual System.Object TargetValue { get; set; }
        public virtual float HowLongToTake { get; set; }

        /// <summary>
        /// This executes each time the lerped value changes.
        /// </summary>
        public virtual UnityAction<System.Object> OnUpdate { get; set; } = delegate { };
    }

    public class TweenArgs<TTargetValue> : TweenArgs
    {
        public new virtual TTargetValue BaseValue
        {
            get { return _baseValue; }
            set
            {
                _baseValue = value;
                base.BaseValue = value;
            }
        }

        protected TTargetValue _baseValue;

        public new virtual TTargetValue TargetValue
        {
            get { return _targetValue; }
            set
            {
                _targetValue = value;
                base.TargetValue = value;
            }
        }
        protected TTargetValue _targetValue;


        /// <summary>
        /// This executes each time the lerped value changes.
        /// </summary>
        public new virtual UnityAction<TTargetValue> OnUpdate { get; set; } = delegate { };
    }

    public class TweenArgs<TTweenTarget, TTargetValue>: TweenArgs<TTargetValue> where TTweenTarget : class
    {
        public virtual TTweenTarget Target { get; set; }

        // Given how generic params work, we can't implement an OnComplete field here and
        // have it work as intended. Thus, we need to implement those in each subclass
        // of this one

        public virtual void Reset()
        {
            Target = null;
            HowLongToTake = 0;
        }
    }
}