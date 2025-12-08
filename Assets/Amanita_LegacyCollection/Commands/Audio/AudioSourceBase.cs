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
            return CommandColors.Audio;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(audioSource.VarRef, variable) ||
                base.HasReference(variable);
        }
    }
}