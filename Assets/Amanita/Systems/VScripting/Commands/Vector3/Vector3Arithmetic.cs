using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Vector3 add, sub, mul, div arithmetic
    /// </summary>
    [CommandInfo("Vector3",
                 "Arithmetic",
                 "Vector3 add, sub, mul, div arithmetic")]
    [AddComponentMenu("")]
    public class Vector3Arithmetic : Command
    {
        [SerializeField]
        protected Vector3Data lhs, rhs, output;

        public enum Operation
        {
            Add,
            Sub,
            Mul,
            Div
        }

        [SerializeField]
        protected Operation operation = Operation.Add;

        public override void OnEnter()
        {
            Vector3 tmp;
            switch (operation)
            {
                case Operation.Add:
                    output.Value = lhs.Value + rhs.Value;
                    break;
                case Operation.Sub:
                    output.Value = lhs.Value - rhs.Value;
                    break;
                case Operation.Mul:
                    tmp = lhs.Value;
                    tmp.Scale(rhs.Value);
                    output.Value = tmp;
                    break;
                case Operation.Div:
                    tmp = lhs.Value;
                    tmp.Scale(new Vector3(1.0f / rhs.Value.x,
                        1.0f / rhs.Value.y,
                        1.0f / rhs.Value.z));
                    output.Value = tmp;
                    break;
                default:
                    break;
            }
            Continue();
        }

        public override string GetSummary()
        {
            if (output.vector3Ref == null)
            {
                return "Error: no output set";
            }

            return operation.ToString() + ": stored in " + output.vector3Ref.Key;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(lhs.VarRef, variable) || 
                ReferenceEquals(rhs.VarRef, variable) || 
                ReferenceEquals(output.VarRef, variable))
                return true;

            return false;
        }
    }
}
