using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// Container for a AudioClip variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioClip), typeof(IVariable<AudioClip>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class AudioClipData : VariableData<AudioClip>
	{
		[SerializeField]
		[VariableProperty("<Value>", typeof(AudioClipVariable))]
		public AudioClipVariable audioClipRef;

		public static implicit operator AudioClip(AudioClipData AudioClipData)
		{
			return AudioClipData.Value;
		}

		protected override Variable LegacyVarRef
		{
			get => audioClipRef;
			set
			{
				audioClipRef = value as AudioClipVariable;
				base.LegacyVarRef = value;
			}
		}

		public AudioClipData() : base(default) { }

		public AudioClipData(AudioClip startVal) : base(startVal) { }

	}

	/// <summary>
	/// Container for an AudioSource variable reference or constant value.
	/// </summary>
	[System.Serializable]
	[VariableData(typeof(AudioSource), typeof(IVariable<AudioSource>))]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class AudioSourceData : VariableData<AudioSource>
	{
		[SerializeField]
		[VariableProperty("<Value>", typeof(AudioSourceVariable))]
		public AudioSourceVariable audioSourceRef;

		public static implicit operator AudioSource(AudioSourceData audioSourceData)
		{
			return audioSourceData.Value;
		}

		public AudioSourceData() : base(default) { }
		public AudioSourceData(AudioSource startVal = null) : base(startVal) { }

		protected override Variable LegacyVarRef
		{
			get => audioSourceRef;
			set => audioSourceRef = value as AudioSourceVariable;
		}

	}
}