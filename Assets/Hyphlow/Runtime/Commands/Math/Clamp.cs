using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to contain a value between a lower and upper bound, with optional wrapping modes
    /// </summary>
    [CommandInfo("Math",
                 "Clamp",
                 "Command to contain a value between a lower and upper bound, with optional wrapping modes")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Clamp : Command
    {
        public enum Mode
        {
            Clamp,
            Repeat,
            PingPong
        }
        
        [SerializeField]
        protected Mode mode = Mode.Clamp;

        //[Tooltip("LHS Value ")]
        [SerializeField]
        protected FloatData lower, upper, value;

        [Tooltip("Result put here, if using pingpong don't use the same var for value as outValue.")]
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(lower);
            _variableDataCache.Add(upper);
            _variableDataCache.Add(value);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            var l = lower.Value;
            var u = upper.Value;
            var v = value.Value;

            switch (mode)
            {
                case Mode.Clamp:
                    outValue.Value = Mathf.Clamp(value.Value, lower.Value, upper.Value);
                    break;
                case Mode.Repeat:
                    outValue.Value = (Mathf.Repeat(v - l, u - l)) + l;
                    break;
                case Mode.PingPong:
                    outValue.Value = (Mathf.PingPong(v - l, u - l)) + l;
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (outValue.floatRef == null)
                return "Error: no output value selected";

            return outValue.floatRef.Key + " = " + Mode.Clamp.ToString() + (mode != Mode.Clamp ? " & " + mode.ToString() : "");
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(lower.VarRef, variable) || 
                ReferenceEquals(upper.VarRef, variable) || 
                ReferenceEquals(value.VarRef, variable) ||
                   ReferenceEquals(outValue.VarRef, variable);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
