using UnityEngine;
using UnityEngine.Audio;


namespace Amanita.VScripting
{
	/// <summary>
	/// AudioMixer variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioMixer", typeof(AudioMixer))]
	[AddComponentMenu("")]
	[System.Serializable]
	public class AudioMixerVariable : VariableBase<AudioMixer>
	{ }

	/// <summary>
	/// Container for a AudioMixer variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioMixer), typeof(AudioMixerVariable))]
	public class AudioMixerData : VariableData<AudioMixer>
	{
		public AudioMixerData() : base(default) { }
		public AudioMixerData(AudioMixer startVal = null) : base(startVal) { }

	}
}