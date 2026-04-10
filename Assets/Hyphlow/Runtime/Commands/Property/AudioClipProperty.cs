using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Get or Set a property of a AudioClip component
	/// </summary>
	[CommandInfo("Property",
				 "AudioClip",
				 "Get or Set a property of a AudioClip component")]
	[AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class AudioClipProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			Length, 
			Samples, 
			Channels, 
			Frequency, 
			PreloadAudioData, 
			Ambisonic, 
			LoadInBackground, 
		}


		[SerializeField]
		protected Property property;

		[SerializeField]
		protected AudioClipData audioClipData;

		[SerializeField]
		[VariableProperty(typeof(FloatVariable),
						  typeof(IntegerVariable),
						  typeof(BooleanVariable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var iof = inOutVar as FloatVariable;
			var ioi = inOutVar as IntegerVariable;
			var iob = inOutVar as BooleanVariable;


			var target = audioClipData.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.Length:
							iof.Value = target.length;
							break;
						case Property.Samples:
							ioi.Value = target.samples;
							break;
						case Property.Channels:
							ioi.Value = target.channels;
							break;
						case Property.Frequency:
							ioi.Value = target.frequency;
							break;
						case Property.PreloadAudioData:
							iob.Value = target.preloadAudioData;
							break;
						case Property.Ambisonic:
							iob.Value = target.ambisonic;
							break;
						case Property.LoadInBackground:
							iob.Value = target.loadInBackground;
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
			if (audioClipData.Value == null)
			{
				return "Error: no audioClip set";
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
			if (ReferenceEquals(audioClipData.VarRef, variable) || inOutVar == variable)
				return true;

			return false;
		}
	}
}
