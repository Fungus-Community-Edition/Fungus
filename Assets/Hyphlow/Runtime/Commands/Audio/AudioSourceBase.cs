using UnityEngine;

namespace AtMycelia.Hyphlow
{
    [AddComponentMenu("")]
    public class AudioSourceBase : Command
    {
        [SerializeField] protected AudioSourceData _audioSource;

        public override string GetSummary()
        {
            if (_audioSource.Value == null)
                return "Error: no source set";

            return _audioSource.Value.name;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_audioSource.VarRef, variable) ||
                base.HasReference(variable);
        }
    }
}