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

		//public override IVariable VarRef
		//{
		//	get { return audioClipRef; }
		//	set
		//	{
		//		if (value == null) { audioClipRef = null; return; }

		//		if (value.ContentType.Equals(this.ContentType))
		//		{
		//			audioClipRef = value as AudioClipVariable;
		//		}
		//		else
		//		{
		//			string errorMessage = $"This can only accept a variable type that holds content of type {ContentType.Name}.";
		//			throw new System.InvalidCastException(errorMessage);
		//		}

		//	}
		//}

		public override void Refresh()
		{
			varRef ??= audioClipRef;
		}
	}
}