using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calculates the inverse lerp, the percentage a value is between two others.
    /// </summary>
    [CommandInfo("Math",
                 "InvLerp",
                 "Calculates the inverse lerp, the percentage a value is between two others.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class InvLerp : Command
    {
        [Tooltip("Clamp percentage to 0-1?")]
        [SerializeField]
        protected bool clampResult = true;

        //[Tooltip("LHS Value ")]
        [SerializeField]
        protected FloatData a, b, value;

        //[Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(a);
            _variableDataCache.Add(b);
            _variableDataCache.Add(value);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            if (clampResult)
                outValue.Value = Mathf.InverseLerp(a.Value, b.Value, value.Value);
            else
                outValue.Value = (value.Value - a.Value) / (b.Value - a.Value);

            Continue();
        }

        public override string GetSummary()
        {
            if (outValue.floatRef == null)
                return "Error: no out value selected";

            return outValue.floatRef.Key + " = [" + a.Value.ToString() + "-" + b.Value.ToString() + "]";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(a.VarRef, variable) ||
                ReferenceEquals(b.VarRef, variable) ||
                ReferenceEquals(value.VarRef, variable) ||
                   ReferenceEquals(outValue.VarRef, variable);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
