using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioClip),
        typeDisplayName: "AudioClip",
        pathToTemplate: "UIToolkitTemplates/VarRows/AudioClipVariableRow")]
    public class AudioClipRowVisualHandler : RowVisualHandler<AudioClip>
    {
    }

}