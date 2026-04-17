using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// The block will execute when a 3d physics trigger matching some basic conditions is met. 
	/// </summary>
	[EventHandlerInfo("MonoBehaviour",
					  "Trigger",
					  "The block will execute when a 2d or 3d physics trigger matching some basic conditions is met.")]
	[AddComponentMenu("")]
	[MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
	public class Trigger : BasePhysicsEventHandler
	{
		[Tooltip("Optional variable to store the collider that caused the trigger to occur.")]
		[ContentTypeConstraint(typeof(Collider), typeof(Collider2D))]
		[SerializeField] protected VariableReference _colliderVar = new VariableReference();

		#region ThreeD

		protected override void OnTriggerEnterResponse(Collider col)
		{
			ProcessCollider(PhysicsMessageType.Enter, col);
		}

		protected void ProcessCollider(PhysicsMessageType from, Component other)
		{
			bool rightMessageType = (from & FireOn) != 0;
			if (rightMessageType && _tagFilter.PassesFilter(other.tag))
			{
				UpdateColliderVar(other);
				ExecuteBlock();
			}
		}

		protected virtual void UpdateColliderVar(Component col)
		{
			bool weHaveVar = _colliderVar != null && _colliderVar.Variable != null;
			if (!weHaveVar)
			{
				return;
			}

			IVariable var = _colliderVar.Variable;
			bool canAssignValue = var.ContentType.IsAssignableFrom(col.GetType());
			if (!canAssignValue)
			{
				return;
			}

			_colliderVar.SetValue(col);
		}

		protected override void OnTriggerStayResponse(Collider col)
		{
			ProcessCollider(PhysicsMessageType.Stay, col);
		}

		protected override void OnTriggerExitResponse(Collider col)
		{
			ProcessCollider(PhysicsMessageType.Exit, col);
		}

		#endregion

		#region TwoD
		protected override void OnTriggerEnterTwoDResponse(Collider2D col)
		{
			ProcessCollider(PhysicsMessageType.Enter, col);
		}


		protected override void OnTriggerExitTwoDResponse(Collider2D col)
		{
			ProcessCollider(PhysicsMessageType.Exit, col);
		}

		protected override void OnTriggerStayTwoDResponse(Collider2D col)
		{
			ProcessCollider(PhysicsMessageType.Stay, col);
		}

		#endregion

		protected override void OnAfterDeserializeBackwardsCompat()
		{
			base.OnAfterDeserializeBackwardsCompat();
			if (colliderVar != null)
			{
				_colliderVar.Variable = colliderVar;
				colliderVar = null;
			}
		}

		[SerializeField] [HideInInspector] protected ColliderVariable colliderVar;
	}
}