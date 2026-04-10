using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Store Input.GetAxis in a variable
    /// </summary>
    [CommandInfo("Input",
                 "GetAxis",
                 "Store Input.GetAxis in a variable")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Hyphlow")]
    public class GetAxis : Command
    {
        [SerializeField]
        protected StringData axisName;

        [Tooltip("If true, calls GetAxisRaw instead of GetAxis")]
        [SerializeField]
        protected bool axisRaw = false;

        [Tooltip("Float to store the value of the GetAxis")]
        [SerializeField]
        protected FloatData outValue;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(axisName);
            _variableDataCache.Add(outValue);
        }

        public override void OnEnter()
        {
            if (axisRaw)
            {
                outValue.Value = Input.GetAxisRaw(axisName.Value);
            }
            else
            {
                outValue.Value = Input.GetAxis(axisName.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            return axisName + (axisRaw ? " Raw" : "");
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(axisName.VarRef, variable) || ReferenceEquals(outValue.VarRef, variable))
                return true;

            return false;
        }
    }
}
