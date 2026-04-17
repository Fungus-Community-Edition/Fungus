using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Vector3 add, sub, mul, div arithmetic
    /// </summary>
    [CommandInfo("Vector3",
                 "Arithmetic",
                 "Vector3 add, sub, mul, div arithmetic")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Vector3Arithmetic : Command
    {
        [SerializeField]
        [FormerlySerializedAs("lhs")]
        protected Vector3Data _lhs;
        [SerializeField]
        [FormerlySerializedAs("rhs")]
        protected Vector3Data _rhs;
        [SerializeField]
        [ContentTypeConstraint(typeof(Vector3))]
        protected VariableReference _outputVar;

        public enum Operation
        {
            Add,
            Sub,
            Mul,
            Div
        }

        [SerializeField]
        [FormerlySerializedAs("operation")]
        protected Operation _operation = Operation.Add;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_lhs);
            _variableDataCache.Add(_rhs);
        }

        public override void OnEnter()
        {
            Vector3 tmp;
            switch (_operation)
            {
                case Operation.Add:
                    _outputVar.SetValue(_lhs.Value + _rhs.Value);
                    break;
                case Operation.Sub:
                    _outputVar.SetValue(_lhs.Value - _rhs.Value);
                    break;
                case Operation.Mul:
                    tmp = _lhs.Value;
                    tmp.Scale(_rhs.Value);
                    _outputVar.SetValue(tmp);
                    break;
                case Operation.Div:
                    tmp = _lhs.Value;
                    tmp.Scale(new Vector3(1.0f / _rhs.Value.x,
                        1.0f / _rhs.Value.y,
                        1.0f / _rhs.Value.z));
                    _outputVar.SetValue(tmp);
                    break;
                default:
                    break;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (_outputVar.Variable == null)
            {
                return "Error: no output set";
            }

            string result = $"{_operation} {GetSummaryString(_lhs)} and {GetSummaryString(_rhs)}, put into {_outputVar.Variable.Key}";
            return result;
        }

        private static string GetSummaryString(Vector3Data var3Data)
        {
            return var3Data.VarRef != null ? 
                var3Data.VarRef.Key : 
                var3Data.Value.ToString();
        }
        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(_lhs.VarRef, variable) || 
                ReferenceEquals(_rhs.VarRef, variable) || 
                ReferenceEquals(_outputVar.Variable, variable))
                return true;

            return false;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (output != null)
            {
                if (output.RepresentingVar)
                {
                    _outputVar.Variable = output.VarRef;
                }
                output = null;
            }
        }

        [SerializeField]
        [HideInInspector]
        protected Vector3Data output;
    }
}
