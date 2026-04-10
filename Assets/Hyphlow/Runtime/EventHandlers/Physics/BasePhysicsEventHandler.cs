using AtMycelia.Events;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Base class for all of our physics event handlers
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom("AtMycelia.Amanita.VScripting.EventHandlers")]
	public abstract class BasePhysicsEventHandler : EventHandler
	{
		[SerializeField] protected VarDataTagFilter _tagFilter = new VarDataTagFilter();

		[Tooltip("The GameObject to watch for trigger events. If left empty, the trigger events of the " +
			"GameObject this EventHandler is attached to will be watched.")]
		[SerializeField] protected GameObjectData _toWatch = new GameObjectData();

		[Flags]
		public enum PhysicsMessageType
		{
			Enter = 1 << 0,
			Stay = 1 << 1,
			Exit = 1 << 2,
		}

		[Tooltip("Which of the physics messages do we trigger on.")]
		[SerializeField]
		[EnumFlag]
		protected PhysicsMessageType FireOn = PhysicsMessageType.Enter;

		protected override void Awake()
		{
			base.Awake();
			if (Application.IsPlaying(this))
			{
				AttachNotifierToRightObject();
			}
		}

		protected virtual void AttachNotifierToRightObject()
		{
			GameObject toAttachTo = this.gameObject;

			if (_toWatch != null && _toWatch.Value != null)
			{
				toAttachTo = _toWatch.Value;
			}

			_notifier = toAttachTo.GetOrAddComponent<PhysicsEventNotifier>();
		}

		private PhysicsEventNotifier _notifier;

		protected bool PassesTagFilter(string tag)
		{
			return _tagFilter.PassesFilter(tag);
		}

		protected void ProcessTagFilterAndExecute(string tagOnOther)
		{
			if (_tagFilter.PassesFilter(tagOnOther))
			{
				ExecuteBlock();
			}
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
				_notifier.TriggerEnter += OnTriggerEnterResponse;
				_notifier.TriggerStay += OnTriggerStayResponse;
				_notifier.TriggerExit += OnTriggerExitResponse;

				_notifier.TriggerEnter2D += OnTriggerEnterTwoDResponse;
				_notifier.TriggerStay2D += OnTriggerStayTwoDResponse;
				_notifier.TriggerExit2D += OnTriggerExitTwoDResponse;
			}
			else
			{
				_notifier.TriggerEnter -= OnTriggerEnterResponse;
				_notifier.TriggerStay -= OnTriggerStayResponse;
				_notifier.TriggerExit -= OnTriggerExitResponse;

				_notifier.TriggerEnter2D -= OnTriggerEnterTwoDResponse;
				_notifier.TriggerStay2D -= OnTriggerStayTwoDResponse;
				_notifier.TriggerExit2D -= OnTriggerExitTwoDResponse;
			}
		}

		// Callbacks for subclasses to override
		#region ThreeD
		protected virtual void OnTriggerEnterResponse(Collider col)
		{

		}

		protected virtual void OnTriggerStayResponse(Collider col)
		{

		}

		protected virtual void OnTriggerExitResponse(Collider col)
		{
			
		}

		#endregion

		#region TwoD
		protected virtual void OnTriggerExitTwoDResponse(Collider2D col)
		{
		}

		protected virtual void OnTriggerStayTwoDResponse(Collider2D col)
		{
			
		}

		protected virtual void OnTriggerEnterTwoDResponse(Collider2D col)
		{
			
		}
		#endregion

		protected override void OnValidate()
		{
			base.OnValidate();

			if (_toWatch != null && !_toWatch.RepresentingVar && _toWatch.Value == null)
			{
				_toWatch.Value = this.gameObject;
			}
		}

		// Handle backward compatibility if needed
		protected override void OnAfterDeserializeBackwardsCompat()
		{
			base.OnAfterDeserializeBackwardsCompat();
			if (tagFilter != null && tagFilter.Length > 0)
			{
				_tagFilter = new VarDataTagFilter(tagFilter);
				tagFilter = null;
			}
		}

		[SerializeField]
		[HideInInspector]
		protected string[] tagFilter;
	}
}