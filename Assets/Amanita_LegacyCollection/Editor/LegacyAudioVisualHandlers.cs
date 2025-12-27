using UnityEngine.Audio;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioMixer),
        typeDisplayName: "AudioMixer",
        pathToTemplate: "UIToolkitTemplates/VarRows/Audio/AudioMixerVariableRow")]
    public class AudioMixerRowVisualHandler : RowVisualHandler<AudioMixer>
    {
    }

    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioMixerGroup),
        typeDisplayName: "AudioMixerGroup",
        pathToTemplate: "UIToolkitTemplates/VarRows/Audio/AudioMixerGroupVariableRow")]
    public class AudioMixerGroupRowVisualHandler : RowVisualHandler<AudioMixerGroup>
    {

    }

    [RowVisualHandler(menuName: "Audio",
        contentType: typeof(AudioMixerSnapshot),
        typeDisplayName: "AudioMixerSnapshot",
        pathToTemplate: "UIToolkitTemplates/VarRows/Audio/AudioMixerSnapshotVariableRow")]
    public class AudioMixerSnapshotRowVisualHandler : RowVisualHandler<AudioMixerSnapshot>
    {

    }
}