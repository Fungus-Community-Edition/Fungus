using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a Transform component
	/// </summary>
	[CommandInfo("Property",
				 "Transform",
				 "Get or Set a property of a Transform component")]
	[AddComponentMenu("")]
	[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class TransformProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			ChildCount, 
			EulerAngles, 
			Forward, 
			HasChanged, 
			HierarchyCapacity, 
			HierarchyCount, 
			LocalEulerAngles, 
			LocalPosition, 
			LocalScale, 
			LossyScale, 
			Parent, 
			Position, 
			Right, 
			Root, 
			Up, 
			Rotation, 
			LocalRotation, 
			WorldToLocalMatrix, 
			LocalToWorldMatrix, 
		}

		[SerializeField]
		protected Property property = Property.Position;

		[SerializeField]
		protected TransformData transformData;

		[SerializeField]
		[ContentTypeConstraint(typeof(Vector3), typeof(Transform), typeof(int), typeof(bool))]
		protected VariableReference _inOutVar;

		protected override void RefreshVariableDataCache()
		{
			base.RefreshVariableDataCache();
			_variableDataCache.Add(transformData);
		}

		public override void OnEnter()
		{
			var iov = _inOutVar.Variable as IVariable<Vector3>;
			var iot = _inOutVar.Variable as IVariable<Transform>;
			var ioi = _inOutVar.Variable as IVariable<int>;
			var iob = _inOutVar.Variable as IVariable<bool>;

			var target = transformData.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.Position:
							iov.Value = target.position;
							break;
						case Property.LocalPosition:
							iov.Value = target.localPosition;
							break;
						case Property.EulerAngles:
							iov.Value = target.eulerAngles;
							break;
						case Property.LocalEulerAngles:
							iov.Value = target.localEulerAngles;
							break;
						case Property.Right:
							iov.Value = target.right;
							break;
						case Property.Up:
							iov.Value = target.up;
							break;
						case Property.Forward:
							iov.Value = target.forward;
							break;
						case Property.LocalScale:
							iov.Value = target.localScale;
							break;
						case Property.Parent:
							iot.Value = target.parent;
							break;
						case Property.Root:
							iot.Value = target.root;
							break;
						case Property.ChildCount:
							ioi.Value = target.childCount;
							break;
						case Property.LossyScale:
							iov.Value = target.lossyScale;
							break;
						case Property.HasChanged:
							iob.Value = target.hasChanged;
							break;
						case Property.HierarchyCapacity:
							ioi.Value = target.hierarchyCapacity;
							break;
						case Property.HierarchyCount:
							ioi.Value = target.hierarchyCount;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}
					break;
				case GetSet.Set:
					switch (property)
					{
						case Property.Position:
							target.position = iov.Value;
							break;
						case Property.LocalPosition:
							target.localPosition = iov.Value;
							break;
						case Property.EulerAngles:
							target.eulerAngles = iov.Value;
							break;
						case Property.LocalEulerAngles:
							target.localEulerAngles = iov.Value;
							break;
						case Property.Right:
							target.right = iov.Value;
							break;
						case Property.Up:
							target.up = iov.Value;
							break;
						case Property.Forward:
							target.forward = iov.Value;
							break;
						case Property.LocalScale:
							target.localScale = iov.Value;
							break;
						case Property.Parent:
							target.parent = iot.Value;
							break;
						case Property.HasChanged:
							target.hasChanged = iob.Value;
							break;
						case Property.HierarchyCapacity:
							target.hierarchyCapacity = ioi.Value;
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
			if (transformData.Value == null)
			{
				return "Error: no transform set";
			}
			if (_inOutVar == null)
			{
				return "Error: no variable set to push or pull data to or from";
			}

			//We could do further checks here, eg, you have selected childcount but set a vec3variable
			string result = getOrSet.ToString() + " " + property.ToString();
			if (_inOutVar.Variable != null)
			{
				if (getOrSet == GetSet.Get)
				{
					result += $" from {transformData.Value.name} & put into ";
				}
				else
				{
					result += " to ";
				}
				result +=  _inOutVar.Variable.Key;
			}
			return result;
		}

		public override Color GetButtonColor()
		{
			return CommandColors.Flow;
		}

		public override bool HasReference(Variable variable)
		{
			if (ReferenceEquals(transformData.VarRef, variable) || 
				ReferenceEquals(_inOutVar.Variable, variable))
				return true;

			return false;
		}

		public override void ApplyBackwardsCompatibility()
		{
			base.ApplyBackwardsCompatibility();

			if (inOutVarOld != null)
			{
				_inOutVar.Variable = inOutVarOld;
				inOutVarOld = null;
			}
		}

		[SerializeField]
		[HideInInspector]
		[FormerlySerializedAs("inOutVar")]
		protected Variable inOutVarOld;

	}
}
