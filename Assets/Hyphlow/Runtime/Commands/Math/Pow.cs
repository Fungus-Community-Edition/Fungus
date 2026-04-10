using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Raise a value to the power of another
    /// </summary>
    [CommandInfo("Math",
                 "Pow",
                 "Raise a value to the power of another.")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Pow : Command
    {
        [SerializeField]
        protected FloatData baseValue, exponentValue;

        [Tooltip("Where the result of the function is stored.")]
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(baseValue);
            _variableDataCache.Add(exponentValue);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            outValue.Value = Mathf.Pow(baseValue.Value, exponentValue.Value);

            Continue();
        }

        public override string GetSummary()
        {
            if (outValue.floatRef == null)
                return "Error: No out value selected";

            return outValue.floatRef.Key + " = " + baseValue.Value.ToString() + "^" + exponentValue.Value.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(baseValue.VarRef, variable) || 
                ReferenceEquals(exponentValue.VarRef, variable) ||
                   ReferenceEquals(outValue.VarRef, variable);
        }
    }
}
