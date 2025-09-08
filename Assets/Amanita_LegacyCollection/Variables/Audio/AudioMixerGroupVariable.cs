using UnityEngine;
using UnityEngine.Audio;

namespace Amanita.VScripting
{
	/// <summary>
	/// AudioMixerGroup variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioMixerGroup", typeof(AudioMixerGroup))]
	[AddComponentMenu("")]
	[System.Serializable]
	public class AudioMixerGroupVariable : VariableBase<AudioMixerGroup>
	{ }

	/// <summary>
	/// Container for a AudioMixerGroup variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioMixerGroup), typeof(AudioMixerGroupVariable))]
	public class AudioMixerGroupData : VariableData<AudioMixerGroup>
	{
		[SerializeField]
		[VariableProperty("<Value>", typeof(AudioMixerGroupVariable))]
		public AudioMixerGroupVariable audioMixerGroupRef;

		public static implicit operator AudioMixerGroup(AudioMixerGroupData AudioMixerGroupData)
		{
			return AudioMixerGroupData.Value;
		}

		public AudioMixerGroupData() : base(default) { }
		public AudioMixerGroupData(AudioMixerGroup startVal = null) : base(startVal) { }

		public override IVariable VarRef
		{
			get { return audioMixerGroupRef; }
			set
			{
				if (value == null) { audioMixerGroupRef = null; return; }

				if (value.ContentType.Equals(this.ContentType))
				{
					audioMixerGroupRef = value as AudioMixerGroupVariable;
				}
				else
				{
					string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
					throw new System.InvalidCastException(errorMessage);
				}

			}
		}

	}
}