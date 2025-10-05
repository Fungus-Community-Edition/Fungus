using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Add force to a Rigidbody2D
    /// </summary>
    [CommandInfo("Rigidbody2D",
                 "AddForce2D",
                 "Add force to a Rigidbody2D")]
    [AddComponentMenu("")]
    public class AddForce2D : Command, ISerializationCallbackReceiver
    {
        [SerializeField]
        protected Rigidbody2DData rb;

        [SerializeField]
        protected ForceMode2D forceMode = ForceMode2D.Force;

        public enum ForceFunction
        {
            AddForce,
            AddForceAtPosition,
            AddRelativeForce
        }

        [SerializeField]
        protected ForceFunction forceFunction = ForceFunction.AddForce;

        [Tooltip("Vector of force to be added")]
        [SerializeField]
        protected Vector2Data force;

        [Tooltip("Scale factor to be applied to force as it is used.")]
        [SerializeField]
        protected FloatData forceScaleFactor = new FloatData(1);

        [Tooltip("World position the force is being applied from. Used only in AddForceAtPosition")]
        [SerializeField]
        protected Vector2Data atPosition;

        public override void OnEnter()
        {
            switch (forceFunction)
            {
                case ForceFunction.AddForce:
                    rb.Value.AddForce(force.Value * forceScaleFactor.Value, forceMode);
                    break;
                case ForceFunction.AddForceAtPosition:
                    rb.Value.AddForceAtPosition(force.Value * forceScaleFactor.Value, atPosition.Value, forceMode);
                    break;
                case ForceFunction.AddRelativeForce:
                    rb.Value.AddRelativeForce(force.Value * forceScaleFactor.Value, forceMode);
                    break;
                default:
                    break;
            }


            Continue();
        }

        public override string GetSummary()
        {
            return forceMode.ToString() + ": " + force.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(rb.VarRef, variable) ||
                ReferenceEquals(force.VarRef, variable) ||
                ReferenceEquals(forceScaleFactor.floatRef, variable) ||
                ReferenceEquals(atPosition.VarRef, variable))
                return true;

            return false;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            forceScaleFactor ??= new FloatData(1);
        }


    }
}
