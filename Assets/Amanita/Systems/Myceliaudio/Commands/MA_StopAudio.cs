using UnityEngine;
using Amanita.VScripting;

namespace Amanita.Myceliaudio.VScripting
{
    [CommandInfo("Myceliaudio", "MA Stop Audio", "Stops the audio playing in a specific track.")]
    public class MA_StopAudio : MyceliaudioCommand
    {
        public enum StopMode
        {
            Null,
            Stop,
            Pause
        }

        [SerializeField] protected StopMode stopMode = StopMode.Stop;
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected IntegerData track = new IntegerData(0);

        public override void OnEnter()
        {
            base.OnEnter();

            if (stopMode == StopMode.Pause)
            {
                AudioSys.Pause(trackGroup, track);
            }
            else if (stopMode == StopMode.Stop)
            {
                AudioSys.StopPlaying(trackGroup, track);
            }
            else
            {
                Debug.LogWarning($"No proper stop mode set here.");
            }

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

            string result = $"{stopMode} {trackGroup} Tr {trackToDisplay}";
            return result;
        }

    }
}