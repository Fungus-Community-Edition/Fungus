using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Map a value that exists in 1 range of numbers to another.
    /// </summary>
    [CommandInfo("Math",
                 "Map",
                 "Map a value that exists in 1 range of numbers to another.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Map : Command
    {
        //[Tooltip("LHS Value ")]
        [SerializeField]
        protected FloatData initialRangeLower = new FloatData(0), initialRangeUpper = new FloatData(1), value;
        
        [SerializeField]
        protected FloatData newRangeLower = new FloatData(0), newRangeUpper = new FloatData(1);
        
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(initialRangeLower);
            _variableDataCache.Add(initialRangeUpper);
            _variableDataCache.Add(value);
            _variableDataCache.Add(newRangeLower);
            _variableDataCache.Add(newRangeUpper);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            var p = value.Value - initialRangeLower.Value;
            p /= initialRangeUpper.Value - initialRangeLower.Value;

            var res = p * (newRangeUpper.Value - newRangeLower.Value);
            res += newRangeLower.Value;

            outValue.Value = res;

            Continue();
        }

        public override string GetSummary()
        {
            return "Map [" + initialRangeLower.Value.ToString() + "-" + initialRangeUpper.Value.ToString() + "] to [" +
                newRangeLower.Value.ToString() + "-" + newRangeUpper.Value.ToString() + "]";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(initialRangeLower.VarRef, variable) || 
                ReferenceEquals(initialRangeUpper.VarRef, variable) || 
                ReferenceEquals(value.VarRef, variable) ||
                   ReferenceEquals(newRangeLower.VarRef, variable) || 
                   ReferenceEquals(newRangeUpper.VarRef, variable) ||
                   ReferenceEquals(outValue.VarRef, variable);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
