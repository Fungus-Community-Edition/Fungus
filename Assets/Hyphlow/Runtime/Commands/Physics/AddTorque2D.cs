using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Add Torque to a Rigidbody2D
    /// </summary>
    [CommandInfo("Physics/Rigidbody2D",
                 "AddTorque2D",
                 "Add Torque to a Rigidbody2D")]
    [AddComponentMenu("")]
    public class AddTorque2D : Command
    {
        [SerializeField]
        [FormerlySerializedAs("rb")]
        protected RigidbodyTwoDData _rb;

        [SerializeField]
        [FormerlySerializedAs("forceMode")]
        protected ForceMode2D _forceMode = ForceMode2D.Force;

        [Tooltip("Amount of torque to be added")]
        [SerializeField]
        [FormerlySerializedAs("force")]
        protected FloatData _force;

        public override void OnEnter()
        {
            _rb.Value.AddTorque(_force.Value, _forceMode);

            Continue();
        }

        public override string GetSummary()
        {
            if (_rb.Value == null)
            {
                return "Error: rb not set";
            }

            return _forceMode.ToString() + ": " + _force.Value.ToString() + (_force.floatRef != null ? " (" + _force.floatRef.Key + ")" : "");
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Physics;
        }
        
        public override bool HasReference(Variable variable)
        {
            if (_rb.rigidbody2DRef == variable || _force.floatRef == variable)
                return true;

            return false;
        }
    }
}