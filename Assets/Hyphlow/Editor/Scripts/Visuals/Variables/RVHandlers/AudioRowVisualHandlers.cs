using UnityEngine;

namespace AtMycelia.Hyphlow.EditorUtils
{
    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioClip),
        typeDisplayName: "AudioClip",
        pathToTemplate: "UIToolkitTemplates/VarRows/Audio/AudioClipVariableRow")]
    public class AudioClipRowVisualHandler : RowVisualHandler<AudioClip>
    {
    }

    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioSource),
        typeDisplayName: "AudioSource",
        pathToTemplate: "UIToolkitTemplates/VarRows/Audio/AudioSourceVariableRow")]
    public class AudioSourceRowVisualHandler : RowVisualHandler<AudioSource>
    {
    }

}