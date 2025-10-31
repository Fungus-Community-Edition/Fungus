using UnityEngine;

namespace Amanita.VScripting
{
	/// <summary>
	/// Container for a AudioClip variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioClip), typeof(IVariable<AudioClip>))]
	public class AudioClipData : VariableData<AudioClip>
	{
		[SerializeField, SerializeReference]
		[VariableProperty("<Value>", typeof(AudioClipVariable))]
		public AudioClipVariable audioClipRef;

		public static implicit operator AudioClip(AudioClipData AudioClipData)
		{
			return AudioClipData.Value;
		}

		public AudioClipData() : base(default) { }

		public AudioClipData(AudioClip startVal) : base(startVal) { }

		public override void Refresh()
		{
			varRef ??= audioClipRef;
		}
	}

	/// <summary>
	/// Container for an AudioSource variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioSource), typeof(IVariable<AudioSource>))]
	public class AudioSourceData : VariableData<AudioSource>
	{
		[SerializeField, SerializeReference]
		[VariableProperty("<Value>", typeof(AudioSourceVariable))]
		public AudioSourceVariable audioSourceRef;

		public static implicit operator AudioSource(AudioSourceData audioSourceData)
		{
			return audioSourceData.Value;
		}

		public AudioSourceData() : base(default) { }
		public AudioSourceData(AudioSource startVal = null) : base(startVal) { }

		public override void Refresh()
		{
			varRef ??= audioSourceRef;
		}

	}
}