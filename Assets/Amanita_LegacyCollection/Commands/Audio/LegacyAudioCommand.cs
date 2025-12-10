using UnityEngine;

namespace Amanita.VScripting.Legacy
{
    public abstract class LegacyAudioCommand : Command
    {
        [Tooltip("Wait until the sound has finished playing before continuing execution.")]
        [SerializeField] protected bool waitUntilFinished;

        protected virtual void Awake()
        {
            MusicManager.EnsureExists();
        }

        protected static MusicManager MusicManager { get => MusicManager.S; }

        protected virtual void OnTimerReachedZero()
        {
            base.Continue();
        }

        public virtual void DelayedContinue()
        {
            Invoke(nameof(Continue), _delayBeforeContinue);
        }

        protected float _delayBeforeContinue;

    }
}