using UnityEngine;

namespace Amanita.VScripting
{
	/// <summary>
	/// Get or Set a property of a AudioMixer component
	/// </summary>
	[CommandInfo("Property",
				 "AudioMixer",
				 "Get or Set a property of a AudioMixer component")]
	[AddComponentMenu("")]
	public class AudioMixerProperty : BaseVariableProperty
	{
		//generated property
		public enum Property 
		{ 
			OutputAudioMixerGroup, 
		}


		[SerializeField]
		protected Property property;

		[SerializeField]
		protected AudioMixerData audioMixerData;

		[SerializeField]
		[VariableProperty(typeof(AudioMixerGroupVariable))]
		protected Variable inOutVar;

		public override void OnEnter()
		{
			var ioamg = inOutVar as AudioMixerGroupVariable;


			var target = audioMixerData.Value;

			switch (getOrSet)
			{
				case GetSet.Get:
					switch (property)
					{
						case Property.OutputAudioMixerGroup:
							ioamg.Value = target.outputAudioMixerGroup;
							break;
						default:
							Debug.Log("Unsupported get or set attempted");
							break;
					}

					break;

				case GetSet.Set:
					switch (property)
					{
						case Property.OutputAudioMixerGroup:
							target.outputAudioMixerGroup = ioamg.Value;
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
			if (audioMixerData.Value == null)
			{
				return "Error: no audioMixer set";
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
			if (ReferenceEquals(audioMixerData.VarRef, variable) || ReferenceEquals(inOutVar, variable))
				return true;

			return false;
		}
	}
}
