using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Audio", typeof(AudioClip), "AudioClip",
        "_EditorResources/UIToolkitTemplates/VarRows/AudioClipVariableRow")]
    public class AudioClipRowVisualHandler : RowVisualHandler<AudioClip>
    {
    }

}