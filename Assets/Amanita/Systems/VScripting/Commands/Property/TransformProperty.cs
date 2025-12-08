using UnityEngine;

namespace Amanita.VScripting
{
	/// <summary>
	/// Get or Set a property of a Transform component
	/// </summary>
	[CommandInfo("Property",
				 "Transform",
				 "Get or Set a property of a Transform component")]
	[AddComponentMenu("")]
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
		[VariableProperty(typeof(Vector3Variable),
						  typeof(TransformVariable),
						  typeof(IntegerVariable),
						  typeof(BooleanVariable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var iov = inOutVar as Vector3Variable;
			var iot = inOutVar as TransformVariable;
			var ioi = inOutVar as IntegerVariable;
			var iob = inOutVar as BooleanVariable;

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
			if (inOutVar == null)
			{
				return "Error: no variable set to push or pull data to or from";
			}

			//We could do further checks here, eg, you have selected childcount but set a vec3variable

			return getOrSet.ToString() + " " + property.ToString();
		}

		public override Color GetButtonColor()
		{
			return CommandColors.Flow;
		}

		public override bool HasReference(Variable variable)
		{
			if (ReferenceEquals(transformData.VarRef, variable) || inOutVar == variable)
				return true;

			return false;
		}

	}
}
