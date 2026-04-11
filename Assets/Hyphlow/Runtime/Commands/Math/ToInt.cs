using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Command to execute and store the result of a float to int conversion
    /// </summary>
    [CommandInfo("Math",
                 "ToInt",
                 "Command to execute and store the result of a float to int conversion")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class ToInt : Command
    {
        public enum Mode
        {
            RoundToInt,
            FloorToInt,
            CeilToInt,
        }


        [Tooltip("To integer mode; round, floor or ceil.")]
        [SerializeField]
        protected Mode function = Mode.RoundToInt;

        [Tooltip("Value to be passed in to the function.")]
        [SerializeField]
        protected FloatData inValue;

        [Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected IntegerData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(inValue);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            switch (function)
            {
            case Mode.RoundToInt:
                outValue.Value = Mathf.RoundToInt(inValue.Value);
                break;
            case Mode.FloorToInt:
                outValue.Value = Mathf.FloorToInt(inValue.Value);
                break;
            case Mode.CeilToInt:
                outValue.Value = Mathf.CeilToInt(inValue.Value);
                break;
            default:
                break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return function.ToString() + 
                   " in: " + (inValue.floatRef != null ? inValue.floatRef.Key : inValue.Value.ToString()) + 
                   ", out: " + (outValue.integerRef != null ? outValue.integerRef.Key : outValue.Value.ToString()); ;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, inValue.VarRef) || 
                ReferenceEquals(variable, outValue.VarRef);
        }
    }
}
