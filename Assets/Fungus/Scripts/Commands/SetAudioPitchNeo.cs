using UnityEngine;

namespace Fungus
{
    [CommandInfo("Audio",
        "SetAudioPitchNeo",
        "Le tin")]
    [AddComponentMenu("")]
    public class SetAudioPitchNeo : Command
    {
        [Tooltip("Global pitch level for audio played using the Play Music and Play Sound commands")]
        [SerializeField] protected FloatData pitch = new FloatData(1);

        [Tooltip("Time to fade between current pitch level and target pitch level.")]
        [SerializeField] protected FloatData fadeDuration;

        [Tooltip("Wait until the pitch change has finished before executing next command")]
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        #region Public members

        public override void OnEnter()
        {
            System.Action onComplete = () => {
                if (waitUntilFinished)
                {
                    Continue();
                }
            };

            var musicManager = FungusManager.Instance.MusicManager;

            musicManager.SetAudioPitch(pitch, fadeDuration, onComplete);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        public override string GetSummary()
        {
            return "Set to " + pitch + " over " + fadeDuration + " seconds.";
        }

        public override Color GetButtonColor()
        {
            return new Color32(242, 209, 176, 255);
        }

        #endregion
    }
}