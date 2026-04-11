using UnityEngine;


using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a Texture component
	/// </summary>
	[CommandInfo("Property",
				 "Texture",
				 "Get or Set a property of a Texture component")]
	[AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class TextureProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			Width, 
			Height, 
			IsReadable, 
			AnisoLevel, 
			MipMapBias, 
			TexelSize, 
			MipmapCount, 
		}

		
		[SerializeField]
		protected Property property;
		
		[SerializeField]
		[VariableProperty(typeof(TextureVariable))]
		protected TextureVariable textureVar;

		[SerializeField]
		[VariableProperty(typeof(IntegerVariable),
						  typeof(BooleanVariable),
						  typeof(FloatVariable),
						  typeof(Vector2Variable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var ioi = inOutVar as IntegerVariable;
			var iob = inOutVar as BooleanVariable;
			var iof = inOutVar as FloatVariable;
			var iov2 = inOutVar as Vector2Variable;

			var target = textureVar.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
				{
						case Property.Width:
							ioi.Value = target.width;
							break;
						case Property.Height:
							ioi.Value = target.height;
							break;
						case Property.IsReadable:
							iob.Value = target.isReadable;
							break;
						case Property.AnisoLevel:
							ioi.Value = target.anisoLevel;
							break;
						case Property.MipMapBias:
							iof.Value = target.mipMapBias;
							break;
						case Property.TexelSize:
							iov2.Value = target.texelSize;
							break;
						case Property.MipmapCount:
							ioi.Value = target.mipmapCount;
							break;
				default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;
				case GetSet.Set:
					switch (property)
					{
						case Property.Width:
							target.width = ioi.Value;
							break;
						case Property.Height:
							target.height = ioi.Value;
							break;
						case Property.AnisoLevel:
							target.anisoLevel = ioi.Value;
							break;
						case Property.MipMapBias:
							target.mipMapBias = iof.Value;
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
			if (textureVar == null)
			{
				return "Error: no textureVar set";
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
			if (textureVar == variable || inOutVar == variable)
				return true;

			return false;
		}

	}
}
