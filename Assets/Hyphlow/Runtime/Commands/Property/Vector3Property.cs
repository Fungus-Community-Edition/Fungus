using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a Vector3 component
	/// </summary>
	[CommandInfo("Property",
				 "Vector3",
				 "Get or Set a property of a Vector3 component")]
	[AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class Vector3Property : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			X, 
			Y, 
			Z, 
			Normalized, 
			Magnitude, 
			SqrMagnitude, 
		}

		
		[SerializeField]
		protected Property property;
		
		[SerializeField]
		[VariableProperty(typeof(Vector3Variable))]
		protected Vector3Variable vector3Var;

		[SerializeField]
		[VariableProperty(typeof(FloatVariable),
						  typeof(Vector3Variable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var iof = inOutVar as FloatVariable;
			var iov = inOutVar as Vector3Variable;


			var target = vector3Var.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.X:
							iof.Value = target.x;
							break;
						case Property.Y:
							iof.Value = target.y;
							break;
						case Property.Z:
							iof.Value = target.z;
							break;
						case Property.Normalized:
							iov.Value = target.normalized;
							break;
						case Property.Magnitude:
							iof.Value = target.magnitude;
							break;
						case Property.SqrMagnitude:
							iof.Value = target.sqrMagnitude;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				case GetSet.Set:
					switch (property)
					{
						case Property.X:
							target.x = iof.Value;
							break;
						case Property.Y:
							target.y = iof.Value;
							break;
						case Property.Z:
							target.z = iof.Value;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				default:
					break;
			}

			vector3Var.Value = target;

			Continue();
		}

		public override string GetSummary()
		{
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
			if (vector3Var == variable || inOutVar == variable)
				return true;

			return false;
		}

	}
}
