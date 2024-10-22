using UnityEngine;
using UnityEngine.Events;

namespace Fungus
{
    public class TweenArgs<TTweenTarget, TTargetValue> where TTweenTarget : class
    {
        public virtual TTweenTarget Target { get; set; }
        public virtual TTargetValue BaseValue { get; set; }
        public virtual TTargetValue TargetValue { get; set; }
        public virtual float HowLongToTake { get; set; }

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