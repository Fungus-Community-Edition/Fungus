using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Base class for all simple Unary
    /// </summary>
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public abstract class BaseUnaryMathCommand : Command
    {
        [Tooltip("Value to be passed in to the function.")]
        [SerializeField]
        protected FloatData inValue;

        [Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;
        
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override string GetSummary()
        {
            return "in: " + (inValue.VarRef != null ? inValue.VarRef.Key : inValue.Value.ToString()) + 
                   ", out: " + (outValue.VarRef != null ? outValue.VarRef.Key : outValue.Value.ToString());
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, inValue.VarRef) || ReferenceEquals(variable, outValue.VarRef);
        }
    }
}
