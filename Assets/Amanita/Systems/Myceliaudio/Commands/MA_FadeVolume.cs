using UnityEngine;
using Amanita.VScripting;

namespace Amanita.Myceliaudio
{
    [CommandInfo("Myceliaudio", "MA Fade Vol", "Fades the volume of an individual track")]
    public class MA_FadeVolume : MyceliaudioCommand
    {
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected FloatData targetVol = new FloatData();
        [SerializeField] protected FloatData duration = new FloatData(1);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);

        public override void OnEnter()
        {
            base.OnEnter();
            PrepFadeArgs();
            AudioSys.FadeTrackVol(fade);

            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void PrepFadeArgs()
        {
            fade.Track = track;
            fade.TrackGroup = trackGroup;
            fade.FadeDuration = duration;
            fade.TargetValue = targetVol;

            if (waitUntilFinished)
            {
                fade.OnComplete = OnFadeComplete;
            }
            else
            {
                fade.OnComplete = delegate { };
            }
        }

        protected AlterAudioSourceArgs fade = new AlterAudioSourceArgs();

        protected virtual void OnFadeComplete(AlterAudioSourceArgs args)
        {
            Continue();
        }

        public override string GetSummary()
        {
            string trackStr;
            bool trackIsVar = track.integerRef != null;
            if (trackIsVar)
            {
                trackStr = track.integerRef.Key;
            }
            else
            {
                trackStr = track.Value.ToString();
            }

            string volStr;
            bool volumeIsVar = targetVol.floatRef != null;
            if (volumeIsVar)
            {
                volStr = targetVol.floatRef.Key;
            }
            else
            {
                volStr = targetVol.Value.ToString();
            }

            string durStr;
            bool durIsVol = duration.floatRef != null;
            if (durIsVol)
            {
                durStr = duration.floatRef.Key;
            }
            else
            {
                durStr = duration.Value.ToString();
            }

            string result = $"{trackGroup} Tr {trackStr} to {volStr} over {durStr} seconds";

            //Tr {track.Value} to {targetVol.Value} over {duration.Value} seconds";
            return result;
        }

    }
}