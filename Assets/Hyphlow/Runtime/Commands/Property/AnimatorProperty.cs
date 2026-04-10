using UnityEngine;


using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a Animator component
	/// </summary>
	[CommandInfo("Property", 
				 "Animator",
				 "Get or Set a property of a Animator component")]
	[AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class AnimatorProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			IsOptimizable, 
			IsHuman, 
			HasRootMotion, 
			HumanScale, 
			IsInitialized, 
			DeltaPosition, 
			DeltaRotation, 
			Velocity, 
			AngularVelocity, 
			RootPosition, 
			RootRotation, 
			ApplyRootMotion, 
			HasTransformHierarchy, 
			GravityWeight, 
			BodyPosition, 
			BodyRotation, 
			StabilizeFeet, 
			LayerCount, 
			ParameterCount, 
			FeetPivotActive, 
			PivotWeight, 
			PivotPosition, 
			IsMatchingTarget, 
			Speed, 
			TargetPosition, 
			TargetRotation, 
			PlaybackTime, 
			RecorderStartTime, 
			RecorderStopTime, 
			HasBoundPlayables, 
			LayersAffectMassCenter, 
			LeftFeetBottomHeight, 
			RightFeetBottomHeight, 
			LogWarnings, 
			FireEvents, 
			KeepAnimatorControllerStateOnDisable, 
			KeepAnimatorStateOnDisable, 
			WriteDefaultValuesOnDisable
		}

		
		[SerializeField]
		protected Property property;
		
		[SerializeField]
		[VariableProperty(typeof(AnimatorVariable))]
		protected AnimatorVariable animatorVar;

		[SerializeField]
		[VariableProperty(typeof(BooleanVariable),
						  typeof(FloatVariable),
						  typeof(Vector3Variable),
						  typeof(IntegerVariable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var iob = inOutVar as BooleanVariable;
			var iof = inOutVar as FloatVariable;
			var iov = inOutVar as Vector3Variable;
			var ioi = inOutVar as IntegerVariable;


			var target = animatorVar.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.IsOptimizable:
							iob.Value = target.isOptimizable;
							break;
						case Property.IsHuman:
							iob.Value = target.isHuman;
							break;
						case Property.HasRootMotion:
							iob.Value = target.hasRootMotion;
							break;
						case Property.HumanScale:
							iof.Value = target.humanScale;
							break;
						case Property.IsInitialized:
							iob.Value = target.isInitialized;
							break;
						case Property.DeltaPosition:
							iov.Value = target.deltaPosition;
							break;
						case Property.Velocity:
							iov.Value = target.velocity;
							break;
						case Property.AngularVelocity:
							iov.Value = target.angularVelocity;
							break;
						case Property.RootPosition:
							iov.Value = target.rootPosition;
							break;
						case Property.ApplyRootMotion:
							iob.Value = target.applyRootMotion;
							break;
						case Property.HasTransformHierarchy:
							iob.Value = target.hasTransformHierarchy;
							break;
						case Property.GravityWeight:
							iof.Value = target.gravityWeight;
							break;
						case Property.BodyPosition:
							iov.Value = target.bodyPosition;
							break;
						case Property.StabilizeFeet:
							iob.Value = target.stabilizeFeet;
							break;
						case Property.LayerCount:
							ioi.Value = target.layerCount;
							break;
						case Property.ParameterCount:
							ioi.Value = target.parameterCount;
							break;
						case Property.FeetPivotActive:
							iof.Value = target.feetPivotActive;
							break;
						case Property.PivotWeight:
							iof.Value = target.pivotWeight;
							break;
						case Property.PivotPosition:
							iov.Value = target.pivotPosition;
							break;
						case Property.IsMatchingTarget:
							iob.Value = target.isMatchingTarget;
							break;
						case Property.Speed:
							iof.Value = target.speed;
							break;
						case Property.TargetPosition:
							iov.Value = target.targetPosition;
							break;
						case Property.PlaybackTime:
							iof.Value = target.playbackTime;
							break;
						case Property.RecorderStartTime:
							iof.Value = target.recorderStartTime;
							break;
						case Property.RecorderStopTime:
							iof.Value = target.recorderStopTime;
							break;
						case Property.HasBoundPlayables:
							iob.Value = target.hasBoundPlayables;
							break;
						case Property.LayersAffectMassCenter:
							iob.Value = target.layersAffectMassCenter;
							break;
						case Property.LeftFeetBottomHeight:
							iof.Value = target.leftFeetBottomHeight;
							break;
						case Property.RightFeetBottomHeight:
							iof.Value = target.rightFeetBottomHeight;
							break;
						case Property.LogWarnings:
							iob.Value = target.logWarnings;
							break;
						case Property.FireEvents:
							iob.Value = target.fireEvents;
							break;
						case Property.KeepAnimatorStateOnDisable:
							iob.Value = target.keepAnimatorStateOnDisable;
							break;
						case Property.WriteDefaultValuesOnDisable:
							iob.Value = target.writeDefaultValuesOnDisable;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				case GetSet.Set:
					switch (property)
					{
						case Property.RootPosition:
							target.rootPosition = iov.Value;
							break;
						case Property.ApplyRootMotion:
							target.applyRootMotion = iob.Value;
							break;
						case Property.BodyPosition:
							target.bodyPosition = iov.Value;
							break;
						case Property.StabilizeFeet:
							target.stabilizeFeet = iob.Value;
							break;
						case Property.FeetPivotActive:
							target.feetPivotActive = iof.Value;
							break;
						case Property.Speed:
							target.speed = iof.Value;
							break;
						case Property.PlaybackTime:
							target.playbackTime = iof.Value;
							break;
						case Property.RecorderStartTime:
							target.recorderStartTime = iof.Value;
							break;
						case Property.RecorderStopTime:
							target.recorderStopTime = iof.Value;
							break;
						case Property.LayersAffectMassCenter:
							target.layersAffectMassCenter = iob.Value;
							break;
						case Property.LogWarnings:
							target.logWarnings = iob.Value;
							break;
						case Property.FireEvents:
							target.fireEvents = iob.Value;
							break;
						case Property.KeepAnimatorStateOnDisable:
							target.keepAnimatorStateOnDisable = iob.Value;
							break;
						case Property.WriteDefaultValuesOnDisable:
							target.writeDefaultValuesOnDisable = iob.Value;
							break;
				default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				default:
					break;
			}

			Continue();
		}

		public override string GetSummary()
		{
			if (animatorVar == null)
			{
				return "Error: no animatorVar set";
			}
			if (inOutVar == null)
			{
				return "Error: no variable set to push or pull data to or from";
			}

			return getOrSet.ToString() + " " + property.ToString();
		}

		public override Color GetButtonColor()
		{
			return CommandColors.Flow;
		}

		public override bool HasReference(Variable variable)
		{
			if (animatorVar == variable || inOutVar == variable)
				return true;

			return false;
		}

	}
}
