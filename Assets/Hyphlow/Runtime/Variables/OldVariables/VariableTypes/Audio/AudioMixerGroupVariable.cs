using UnityEngine;
using UnityEngine.Audio;

namespace AtMycelia.Hyphlow
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
		public AudioMixerGroupData() : base(default) { }
		public AudioMixerGroupData(AudioMixerGroup startVal = null) : base(startVal) { }

	}
}