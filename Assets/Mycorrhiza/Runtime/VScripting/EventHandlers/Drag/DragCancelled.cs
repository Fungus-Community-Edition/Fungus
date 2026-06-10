using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AtMycelia.Hyphlow;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;
using AtMycelia.Amanita;

namespace AtMycelia.Mycorrhiza
{
	/// <summary>
	/// The block will execute when the player drags an object and releases it without dropping it on a target object.
	/// </summary>
	[EventHandlerInfo("Sprite",
					  "Drag Cancelled",
					  "The block will execute when the player drags an object and releases it without dropping it on a target object.")]
	[AddComponentMenu("")]
	[MovedFrom("AtMycelia.Mycorrhiza.EventHandlers")]
	public class DragCancelled : EventHandler, ISerializationCallbackReceiver
	{
		[ContentTypeConstraint(typeof(Component), typeof(GameObject))]
		[SerializeField] protected VariableReference _draggableRef = new VariableReference();

		[Tooltip("Draggable object to listen for drag events on")]
		[FormerlySerializedAs("draggableObjects")]
		[SerializeField] protected List<Draggable2D> _draggableObjects;

		protected override void ToggleSubs(bool on)
		{
			base.ToggleSubs(on);
			if (on)
			{
				EventDispatcher.AddListener<DragCancelledEvent>(OnDragCancelledEvent);
			}
			else
			{
				EventDispatcher.RemoveListener<DragCancelledEvent>(OnDragCancelledEvent);
			}
		}

		protected virtual void OnDragCancelledEvent(DragCancelledEvent evt)
		{
			OnDragCancelled(evt.DraggableObject);
		}

		#region Compatibility

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (_oldDraggableRef != null)
			{
				_draggableRef.Variable = _oldDraggableRef;
				_oldDraggableRef = null;
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		#endregion Compatibility

		public virtual void OnDragCancelled(Draggable2D draggableObject)
		{
			if (_draggableObjects.Contains(draggableObject))
			{
				if (_oldDraggableRef != null)
				{
					_oldDraggableRef.Value = draggableObject.gameObject;
				}
				ExecuteBlock();
			}
		}

		public override string GetSummary()
		{
			if (_draggableObjects.Count(x => x != null) == 0)
			{
				return "Error: no draggable objects assigned.";
			}

			string summary = "Draggable: ";
			if (this._draggableObjects != null && this._draggableObjects.Count != 0)
			{
				for (int i = 0; i < this._draggableObjects.Count; i++)
				{
					if (_draggableObjects[i] != null)
					{
						summary += _draggableObjects[i].name + ",";
					}
				}
			}
			return summary;
		}

		public override void ApplyBackwardsCompatibility()
		{
			base.ApplyBackwardsCompatibility();
			
		}

		[VariableProperty(typeof(GameObjectVariable))]
		[FormerlySerializedAs("draggableRef")]
		[HideInInspector]
		[SerializeField] protected GameObjectVariable _oldDraggableRef;

		protected override EventDispatcher EventDispatcher => AmanitaManager.S.EventDispatcher;
	}
}