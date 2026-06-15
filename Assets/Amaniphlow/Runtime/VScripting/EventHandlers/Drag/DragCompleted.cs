using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AtMycelia.Hyphlow;
using UnityEngine.Scripting.APIUpdating;
using AtMycelia.Amanita;

namespace AtMycelia.Amaniphlow
{
	/// <summary>
	/// The block will execute when the player drags an object and successfully drops it on a target object.
	///
	/// ExecuteAlways used to get the Compatibility that we need, use of ISerializationCallbackReceiver is error prone
	/// when used on Unity controlled objects as it runs on threads other than main thread.
	/// </summary>
	[EventHandlerInfo("Sprite",
					  "Drag Completed",
					  "The block will execute when the player drags an object and successfully drops it on a target object.")]
	[AddComponentMenu("")]
	[MovedFrom("AtMycelia.Amaniphlow.EventHandlers")]
	public class DragCompleted : EventHandler, ISerializationCallbackReceiver
	{
		[VariableProperty(typeof(GameObjectVariable))]
		[SerializeField] protected GameObjectVariable draggableRef;

		[VariableProperty(typeof(GameObjectVariable))]
		[SerializeField] protected GameObjectVariable targetRef;

		[Tooltip("Draggable object to listen for drag events on")]
		[HideInInspector]
		[SerializeField] protected Draggable2D draggableObject;

		[SerializeField] protected List<Draggable2D> draggableObjects;

		[Tooltip("Drag target object to listen for drag events on")]
		[HideInInspector]
		[SerializeField] protected Collider2D targetObject;

		[SerializeField] protected List<Collider2D> targetObjects;

		protected override bool ToggleSubsOnlyInRuntime => true;

		protected override void ToggleSubs(bool on)
		{
			base.ToggleSubs(on);
			if (on)
			{
				EventDispatcher.AddListener<DragEndProbeEvent>(OnDragEndProbeEvent);
			}
			else
			{
				EventDispatcher.RemoveListener<DragEndProbeEvent>(OnDragEndProbeEvent);
			}
		}

		private void OnDragEndProbeEvent(DragEndProbeEvent evt)
		{
			if (evt == null || evt.DraggableObject == null)
			{
				return;
			}

			if (draggableObjects == null || targetObjects == null)
			{
				return;
			}

			if (!draggableObjects.Contains(evt.DraggableObject))
			{
				return;
			}

			if (!TryGetMatchedTarget(evt.OverlappingTargets, out Collider2D matchedTarget))
			{
				return;
			}

			// Mark completion first so Draggable2D resolves completion/cancel semantics without
			// depending on EventHandler execution success/failure.
			evt.MarkCompleted(matchedTarget);

			if (draggableRef != null)
			{
				draggableRef.Value = evt.DraggableObject.gameObject;
			}

			if (targetRef != null)
			{
				targetRef.Value = matchedTarget.gameObject;
			}

			ExecuteBlock();
		}

		private bool TryGetMatchedTarget(IReadOnlyList<Collider2D> overlappingTargets, out Collider2D matchedTarget)
		{
			matchedTarget = null;
			if (overlappingTargets == null || targetObjects == null || targetObjects.Count == 0)
			{
				return false;
			}

			for (int i = 0; i < overlappingTargets.Count; i++)
			{
				Collider2D candidate = overlappingTargets[i];
				if (candidate != null && targetObjects.Contains(candidate))
				{
					matchedTarget = candidate;
					return true;
				}
			}

			return false;
		}

		#region Compatibility

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			// presently using Awake due to errors on non main thread access
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		protected override void Awake()
		{
			base.Awake();

			draggableObjects ??= new List<Draggable2D>();
			targetObjects ??= new List<Collider2D>();

			// add any draggable object already present to list for backwards compatibility
			if (draggableObject != null && !draggableObjects.Contains(draggableObject))
			{
				draggableObjects.Add(draggableObject);
			}

			if (targetObject != null && !targetObjects.Contains(targetObject))
			{
				targetObjects.Add(targetObject);
			}

			draggableObject = null;
			targetObject = null;
		}

		#endregion Compatibility

		/// <summary>
		/// Gets the draggable object list this handler cares about.
		/// </summary>
		public virtual List<Draggable2D> DraggableObjects { get { return draggableObjects; } }

		public override string GetSummary()
		{
			if (draggableObjects == null || draggableObjects.Count(x => x != null) == 0)
			{
				return "Error: no draggable objects assigned.";
			}

			if (targetObjects == null || targetObjects.Count(x => x != null) == 0)
			{
				return "Error: no target objects assigned.";
			}

			string summary = "Draggable: ";
			for (int i = 0; i < draggableObjects.Count; i++)
			{
				if (draggableObjects[i] != null)
				{
					summary += draggableObjects[i].name + ",";
				}
			}

			summary += "\nTarget: ";
			for (int i = 0; i < targetObjects.Count; i++)
			{
				if (targetObjects[i] != null)
				{
					summary += targetObjects[i].name + ",";
				}
			}

			return summary;
		}

		protected override EventDispatcher EventDispatcher => AmanitaManager.S.EventDispatcher;
	}
}