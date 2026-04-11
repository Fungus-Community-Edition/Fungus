using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to store the min or max of 2 values
    /// </summary>
    [CommandInfo("Math",
                 "MinMax",
                 "Command to store the min or max of 2 values")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class MinMax : Command
    {
        public enum Function
        {
           Min,
           Max
        }

        [Tooltip("Min Or Max")]
        [SerializeField]
        protected Function function = Function.Min;

        //[Tooltip("LHS Value ")]
        [SerializeField]
        protected FloatData inLHSValue, inRHSValue;

        //[Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(inLHSValue);
            _variableDataCache.Add(inRHSValue);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            switch (function)
            {
                case Function.Min:
                    outValue.Value = Mathf.Min(inLHSValue.Value, inRHSValue.Value);
                    break;
                case Function.Max:
                    outValue.Value = Mathf.Max(inLHSValue.Value, inRHSValue.Value);
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return function.ToString() + " " +
                "out: " + (outValue.floatRef != null ? outValue.floatRef.Key : outValue.Value.ToString()) +
                " [" + inLHSValue.Value.ToString() + " - " + inRHSValue.Value.ToString() + "]";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(inLHSValue.VarRef, variable) || 
                ReferenceEquals(inRHSValue.VarRef, variable) || 
                ReferenceEquals(outValue.VarRef, variable);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
