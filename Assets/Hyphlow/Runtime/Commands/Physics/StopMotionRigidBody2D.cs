using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Stop velocity and angular velocity on a Rigidbody2D
    /// </summary>
    [CommandInfo("Physics/Rigidbody2D",
                 "StopMotion2D",
                 "Stop velocity and angular velocity on a Rigidbody2D")]
    [AddComponentMenu("")]
    public class StopMotionRigidBody2D : Command
    {
        [SerializeField]
        [FormerlySerializedAs("rb")]
        protected RigidbodyTwoDData _rb;

        public enum Motion
        {
            Velocity,
            AngularVelocity,
            AngularAndLinearVelocity
        }

        [SerializeField]
        [FormerlySerializedAs("motionToStop")]
        protected Motion _motionToStop = Motion.AngularAndLinearVelocity;

        public override void OnEnter()
        {
            switch (_motionToStop)
            {
                case Motion.Velocity:
                    #if UNITY_6000
                    _rb.Value.linearVelocity = Vector2.zero;
                    #else
                    rb.Value.velocity = Vector2.zero;
                    #endif
                    break;
                case Motion.AngularVelocity:
                    _rb.Value.angularVelocity = 0;
                    break;
                case Motion.AngularAndLinearVelocity:
                    _rb.Value.angularVelocity = 0;
                    #if UNITY_6000
                    _rb.Value.linearVelocity = Vector2.zero;
                    #else
                    rb.Value.velocity = Vector2.zero;
                    #endif
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            return _motionToStop.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Physics;
        }

        public override bool HasReference(Variable variable)
        {
            if (_rb.rigidbody2DRef == variable)
                return true;

            return false;
        }

    }
}