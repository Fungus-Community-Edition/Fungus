using UnityEngine;


namespace Amanita.VScripting
{
	/// <summary>
	/// AudioClip variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioClip", typeof(AudioClip))]
	[AddComponentMenu("")]
	[System.Serializable]
	public class AudioClipVariable : VariableBase<UnityEngine.AudioClip>
	{ }

	/// <summary>
	/// Container for a AudioClip variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioClip), typeof(AudioClipVariable))]
	public class AudioClipData : VariableData<AudioClip>
	{
		[SerializeField, SerializeReference]
		[VariableProperty("<Value>", typeof(AudioClipVariable))]
		public IVariable<AudioClip> audioClipRef;

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
}