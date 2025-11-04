using UnityEngine;


namespace Amanita.VScripting
{
	/// <summary>
	/// AudioClip variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioClip", typeof(AudioClip), false)]
	[AddComponentMenu("")]
	[System.Serializable]
	public class AudioClipVariable : VariableBase<UnityEngine.AudioClip>
	{ }

	
}
