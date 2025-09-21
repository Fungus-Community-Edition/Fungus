using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Audio", typeof(AudioClip), "AudioClip",
        "UIToolkitTemplates/VarRows/AudioClipVariableRow")]
    public class AudioClipRowVisualHandler : RowVisualHandler<AudioClip>
    {
    }

}