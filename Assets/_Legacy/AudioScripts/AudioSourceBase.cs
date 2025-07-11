using UnityEngine;

namespace Amanita.VScripting
{
    [AddComponentMenu("")]
    public class AudioSourceBase : Command
    {
        [SerializeField] protected AudioSourceData audioSource;

        public override string GetSummary()
        {
            if (audioSource.Value == null)
                return "Error: no source set";

            return audioSource.Value.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return audioSource.audioSourceRef == variable ||
                base.HasReference(variable);
        }
    }
}