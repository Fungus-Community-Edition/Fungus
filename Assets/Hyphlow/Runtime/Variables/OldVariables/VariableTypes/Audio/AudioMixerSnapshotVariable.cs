using UnityEngine;
using UnityEngine.Audio;


namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// AudioMixerSnapshot variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioMixerSnapshot", typeof(AudioMixerSnapshot))]
	[AddComponentMenu("")]
	[System.Serializable]
	public class AudioMixerSnapshotVariable : VariableBase<UnityEngine.Audio.AudioMixerSnapshot>
	{ }

	/// <summary>
	/// Container for a AudioMixerSnapshot variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioMixerSnapshot), typeof(AudioMixerSnapshotVariable))]
	public class AudioMixerSnapshotData : VariableData<AudioMixerSnapshot>
	{
		public AudioMixerSnapshotData() : base(default) { }
	}
}