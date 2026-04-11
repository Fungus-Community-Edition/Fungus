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
	[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
	public class AudioClipVariable : VariableBase<UnityEngine.AudioClip>
	{ }

	
}
