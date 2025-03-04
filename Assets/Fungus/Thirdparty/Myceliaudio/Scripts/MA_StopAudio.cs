using UnityEngine;

namespace Fungus.Myceliaudio
{
    [CommandInfo("Myceliaudio", "MA Stop Audio", "Stops the audio playing in a specific track.")]
    public class MA_StopAudio : MyceliaudioCommand
    {
        [SerializeField] protected IntegerData track = new IntegerData(0);

        public override void OnEnter()
        {
            base.OnEnter();
            AudioSys.StopPlaying(trackGroup, track);
            Continue();
        }

        public override string GetSummary()
        {
            string trackToDisplay = string.Empty;
            if (track.integerRef != null)
            {
                trackToDisplay = track.integerRef.Key;
            }
            else
            {
                trackToDisplay = track.Value.ToString();
            }

            string result = $"In {trackGroup} Tr {trackToDisplay}";
            return result;
        }
    }
}