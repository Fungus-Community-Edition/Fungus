using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Store Input.GetAxis in a variable
    /// </summary>
    [CommandInfo("Input",
                 "GetAxis",
                 "Store Input.GetAxis in a variable")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class GetAxis : Command
    {
        [SerializeField]
        [FormerlySerializedAs("axisName")]
        protected StringData _axisName;

        [Tooltip("If true, calls GetAxisRaw instead of GetAxis")]
        [SerializeField]
        protected BooleanData _useRaw = new BooleanData(false);

        [Tooltip("Float to store the value of the GetAxis")]
        [SerializeField]
        [ContentTypeConstraint(typeof(float), typeof(double))]
        protected VariableReference _outValueRef;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_axisName);
            _variableDataCache.Add(_useRaw);
        }

        public override void OnEnter()
        {
            if (_outValueRef.Variable == null)
            {
                string warningMessage = $"No variable selected to store the result of GetAxis for axis {_axisName.Value}";
                Debug.LogWarning(warningMessage);
                Continue();
            }

            float val;
            if (_useRaw)
            {
                val = Input.GetAxisRaw(_axisName.Value);
            }
            else
            {
                val = Input.GetAxis(_axisName.Value);
            }

            _outValueRef.SetValue(val);

            Continue();
        }

        public override string GetSummary()
        {
            return _axisName + (_useRaw.Value ? " Raw" : "");
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            bool result = ReferenceEquals(_axisName.VarRef, variable) ||
                ReferenceEquals(_useRaw.VarRef, variable) ||
                ReferenceEquals(_outValue.VarRef, variable);

            return result;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldAxisRaw)
            {
                _useRaw.Value = true;
                _oldAxisRaw = false;
            }

            if (_outValue != null)
            {
                if (_outValue.RepresentingVar)
                {
                    _outValueRef.Variable = _outValue.VarRef;
                }

                _outValue = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("axisRaw")]
        [HideInInspector]
        protected bool _oldAxisRaw = false;

        [FormerlySerializedAs("outValue")]
        [SerializeField]
        protected FloatData _outValue;
    }
}
