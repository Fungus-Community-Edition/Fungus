using UnityEngine;
using UnityEngine.Assertions;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Assert on 2 Amanita variable values.
    /// </summary>
    [CommandInfo("Scripting",
                 "Assert",
                 "Assert based on compared values.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public class AssertCommand : Command
    {
        [SerializeField]
        protected StringData message;

        [SerializeField]
        [VariableProperty()]
        protected Variable a, b;

        public enum Method
        {
            AreEqual,
            AreNotEqual,
        }

        [SerializeField]
        protected Method method;

        public override void OnEnter()
        {
            switch (method)
            {
                case Method.AreEqual:
                    Assert.AreEqual(a.GetValue(), b.GetValue());
                    break;

                case Method.AreNotEqual:
                    Assert.AreNotEqual(a.GetValue(), b.GetValue());
                    break;

                default:
                break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (a == null)
                return "Error: No A variable";
            if (b == null)
                return "Error: No B variable";

            return a.Key + " " + method.ToString() + " " + b.Key;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, message.VarRef) ||
                variable == a || variable == b ||
                base.HasReference(variable);
        }
    }
}