



using UnityEngine;
using UnityEngine.Audio;


namespace Amanita.VScripting
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
	public class AudioMixerSnapshotData : VariableData<AudioMixerSnapshot, IVariable<AudioMixerSnapshot>>
	{
		[SerializeField]
		[VariableProperty("<Value>", typeof(AudioMixerSnapshotVariable))]
		public AudioMixerSnapshotVariable audioMixerSnapshotRef;

		public static implicit operator UnityEngine.Audio.AudioMixerSnapshot(AudioMixerSnapshotData AudioMixerSnapshotData)
		{
			return AudioMixerSnapshotData.Value;
		}

		public override IVariable VarRef
		{
			get { return audioMixerSnapshotRef; }
			set
			{
				if (value == null) { audioMixerSnapshotRef = null; return; }

				if (value.ContentType.Equals(this.ContentType))
				{
					audioMixerSnapshotRef = value as AudioMixerSnapshotVariable;
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