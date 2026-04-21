using AtMycelia.Events;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityCollision = UnityEngine.Collision;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// The block will execute when a 3d physics collision matching some basic conditions is met. 
    /// </summary>
    [EventHandlerInfo("MonoBehaviour",
                      "Collision",
                      "The block will execute when a 2d or 3d physics collision matching some basic conditions is met.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
    public class Collision : BasePhysicsEventHandler
    {
        [Tooltip("Optional variable to store the collision that caused the collision to occur.")]
        [ContentTypeConstraint(typeof(UnityCollision), typeof(Collision2D))]
        [SerializeField] protected VariableReference _collisionVar = new VariableReference();

        #region ThreeD

        protected virtual void OnCollisionEnterResponse(UnityCollision col)
        {
            ProcessCollision(PhysicsMessageType.Enter, col);
        }

        protected virtual void OnCollisionStayResponse(UnityCollision col)
        {
            ProcessCollision(PhysicsMessageType.Stay, col);
        }

        protected virtual void OnCollisionExitResponse(UnityCollision col)
        {
            ProcessCollision(PhysicsMessageType.Exit, col);
        }

        protected void ProcessCollision(PhysicsMessageType from, UnityCollision other)
        {
            bool rightMessageType = (from & FireOn) != 0;
            string tagToUse = other.collider != null ? 
                other.collider.tag : 
                other.gameObject.tag;
            if (rightMessageType && _tagFilter.PassesFilter(tagToUse))
            {
                UpdateCollisionVar(other);
                ExecuteBlock();
            }
        }

        #endregion

        #region TwoD

        protected virtual void OnCollisionEnterTwoDResponse(Collision2D col)
        {
            ProcessCollision(PhysicsMessageType.Enter, col);
        }

        protected virtual void OnCollisionStayTwoDResponse(Collision2D col)
        {
            ProcessCollision(PhysicsMessageType.Stay, col);
        }

        protected virtual void OnCollisionExitTwoDResponse(Collision2D col)
        {
            ProcessCollision(PhysicsMessageType.Exit, col);
        }

        protected void ProcessCollision(PhysicsMessageType from, Collision2D other)
        {
            bool rightMessageType = (from & FireOn) != 0;
            string tagToUse = other.collider != null ?
                other.collider.tag :
                other.gameObject.tag;
            if (rightMessageType && _tagFilter.PassesFilter(tagToUse))
            {
                UpdateCollisionVar(other);
                ExecuteBlock();
            }
        }

        #endregion

        protected virtual void UpdateCollisionVar(object col)
        {
            bool weHaveVar = _collisionVar != null && _collisionVar.Variable != null;
            if (!weHaveVar)
            {
                return;
            }

            IVariable var = _collisionVar.Variable;
            bool canAssignValue = var.ContentType.IsAssignableFrom(col.GetType());
            if (!canAssignValue)
            {
                return;
            }

            _collisionVar.SetValue(col);
        }

        protected override void AttachNotifierToRightObject()
        {
            GameObject toAttachTo = this.gameObject;

            if (_toWatch != null && _toWatch.Value != null)
            {
                toAttachTo = _toWatch.Value;
            }

            _notifier = toAttachTo.GetOrAddComponent<PhysicsEventNotifier>();
        }

        protected override void ToggleSubs(bool on)
        {
            base.ToggleSubs(on);
            if (_notifier == null)
            {
                // Expected to trigger when OnDisable gets called before Awake
                return;
            }

            if (on)
            {
                _notifier.CollisionEnter += OnCollisionEnterResponse;
                _notifier.CollisionStay += OnCollisionStayResponse;
                _notifier.CollisionExit += OnCollisionExitResponse;

                _notifier.CollisionEnter2D += OnCollisionEnterTwoDResponse;
                _notifier.CollisionStay2D += OnCollisionStayTwoDResponse;
                _notifier.CollisionExit2D += OnCollisionExitTwoDResponse;
            }
            else
            {
                _notifier.CollisionEnter -= OnCollisionEnterResponse;
                _notifier.CollisionStay -= OnCollisionStayResponse;
                _notifier.CollisionExit -= OnCollisionExitResponse;

                _notifier.CollisionEnter2D -= OnCollisionEnterTwoDResponse;
                _notifier.CollisionStay2D -= OnCollisionStayTwoDResponse;
                _notifier.CollisionExit2D -= OnCollisionExitTwoDResponse;
            }
        }

        protected override void OnAfterDeserializeBackwardsCompat()
        {
            base.OnAfterDeserializeBackwardsCompat();
            if (collisionVar != null)
            {
                _collisionVar.Variable = collisionVar;
                collisionVar = null;
            }
        }

        private PhysicsEventNotifier _notifier;

        [SerializeField] [HideInInspector] protected CollisionVariable collisionVar;
    }
}