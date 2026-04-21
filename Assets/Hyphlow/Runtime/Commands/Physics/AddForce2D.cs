using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Add force to a Rigidbody2D
    /// </summary>
    [CommandInfo("Physics/Rigidbody2D",
                 "AddForce2D",
                 "Add force to a Rigidbody2D")]
    [AddComponentMenu("")]
    public class AddForce2D : Command
    {
        [SerializeField]
        [FormerlySerializedAs("rb")]
        protected RigidbodyTwoDData _rb;

        [SerializeField]
        [FormerlySerializedAs("forceMode")]
        protected ForceMode2D _forceMode = ForceMode2D.Force;

        public enum ForceFunction
        {
            AddForce,
            AddForceAtPosition,
            AddRelativeForce
        }

        [SerializeField]
        protected ForceFunction _forceFunction = ForceFunction.AddForce;

        [Tooltip("Vector of force to be added")]
        [SerializeField]
        [FormerlySerializedAs("force")]
        protected Vector2Data _force;

        [Tooltip("Scale factor to be applied to force as it is used.")]
        [SerializeField]
        [FormerlySerializedAs("forceScaleFactor")]
        protected FloatData _forceScaleFactor = new FloatData(1);

        [Tooltip("World position the force is being applied from. Used only in AddForceAtPosition")]
        [SerializeField]
        [FormerlySerializedAs("atPosition")]
        protected Vector2Data _atPosition;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_rb);
            _variableDataCache.Add(_force);
            _variableDataCache.Add(_forceScaleFactor);
            _variableDataCache.Add(_atPosition);
        }

        public override void OnEnter()
        {
            Vector2 forceToApply = _force.Value * _forceScaleFactor.Value;
            //Debug.Log($"Applying force: {forceToApply} to Rigidbody2D: {_rb.Value.name} with ForceMode2D: {_forceMode}");
            switch (_forceFunction)
            {
                case ForceFunction.AddForce:
                    _rb.Value.AddForce(forceToApply, _forceMode);
                    break;
                case ForceFunction.AddForceAtPosition:
                    _rb.Value.AddForceAtPosition(forceToApply, _atPosition.Value, _forceMode);
                    break;
                case ForceFunction.AddRelativeForce:
                    _rb.Value.AddRelativeForce(forceToApply, _forceMode);
                    break;
                default:
                    break;
            }


            Continue();
        }

        public override string GetSummary()
        {
            return _forceMode.ToString() + ": " + _force.ToString();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Physics;
        }

        public override bool HasReference(Variable variable)
        {
            if (_rb.rigidbody2DRef == variable || _force.vector2Ref == variable || _forceScaleFactor.floatRef == variable ||
                _atPosition.vector2Ref == variable)
                return true;

            return false;
        }

    }
}