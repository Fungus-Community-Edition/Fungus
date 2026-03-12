using UnityEngine;
using UnityEngine.Scripting.APIUpdating;


namespace AtMycelia.Amanita.VScripting
{
	/// <summary>
	/// AudioClip variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioClip", typeof(AudioClip), false)]
	[AddComponentMenu("")]
	[System.Serializable]
	[MovedFrom(true, "Amanita.VScripting", "Amanita.Core")]
	public class AudioClipVariable : VariableBase<UnityEngine.AudioClip>
	{ }

	
}
