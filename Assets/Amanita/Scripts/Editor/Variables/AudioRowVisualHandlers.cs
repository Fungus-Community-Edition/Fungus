using UnityEngine;
using UnityEngine.UIElements;
using EditorObjectField = UnityEditor.UIElements.ObjectField;
using UnityObject = UnityEngine.Object;

namespace Amanita.VScripting.EditorUtils
{
    [RowVisualHandler("Audio", typeof(AudioClip), "AudioClip",
        "_EditorResources/UIToolkitTemplates/VarRows/AudioClipVariableRow")]
    public class AudioClipRowVisualHandler : RowVisualHandler<AudioClip>
    {
        protected override void RegisterVisualElements()
        {
            base.RegisterVisualElements();
            _audioClipField = Root.Q<EditorObjectField>("AudioClipObjectField");
            _audioClipField.objectType = typeof(AudioClip);
        }

        protected EditorObjectField _audioClipField;
    }

}