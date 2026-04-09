using UnityEngine;
using UnityEngine.Scripting.APIUpdating;


namespace AtMycelia.Hyphlow
{
	/// <summary>
	/// AudioClip variable type.
	/// </summary>
	[VariableInfo("Audio", "AudioClip", typeof(AudioClip), false)]
	[AddComponentMenu("")]
	[System.Serializable]
	[MovedFrom(true, "AtMycelia.Amanita.VScripting", "AtMycelia.Amanita.Core")]
	public class AudioClipVariable : VariableBase<UnityEngine.AudioClip>
	{ }

	
}
