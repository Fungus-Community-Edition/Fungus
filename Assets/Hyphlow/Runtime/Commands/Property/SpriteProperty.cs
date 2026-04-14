using UnityEngine;


using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a Sprite component
	/// </summary>
	[CommandInfo("Property",
				 "Sprite",
				 "Get or Set a property of a Sprite component")]
	[AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class SpriteProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			Border, 
			PixelsPerUnit, 
			Pivot, 
			Packed, 
			TextureRectOffset, 
		}

		
		[SerializeField]
		protected Property property;
		
		[SerializeField]
		[VariableProperty(typeof(SpriteVariable))]
		protected SpriteVariable spriteVar;

		[SerializeField]
		[VariableProperty(typeof(FloatVariable),
						  typeof(Vector2Variable),
						  typeof(BooleanVariable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var iof = inOutVar as FloatVariable;
			var iov2 = inOutVar as Vector2Variable;
			var iob = inOutVar as BooleanVariable;

			var target = spriteVar.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.PixelsPerUnit:
							iof.Value = target.pixelsPerUnit;
							break;
						case Property.Pivot:
							iov2.Value = target.pivot;
							break;
						case Property.Packed:
							iob.Value = target.packed;
							break;
						case Property.TextureRectOffset:
							iov2.Value = target.textureRectOffset;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				case GetSet.Set:
					switch (property)
					{
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
			if (spriteVar == null)
			{
				return "Error: no spriteVar set";
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
			if (spriteVar == variable || inOutVar == variable)
				return true;

			return false;
		}

	}
}
