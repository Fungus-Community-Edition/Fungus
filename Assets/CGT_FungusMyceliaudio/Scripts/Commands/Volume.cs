using UnityEngine;
using Fungus;

namespace CGT.FungusExt.Myceliaudio
{
    [CommandInfo("Audio/CGT",
        "Volume",
        "Lets you get or set the volume of the given track.")]
    [AddComponentMenu("")]
    public class Volume : SoundShifter
    {
        protected override AudioArgs GetAudioArgs()
        {
            var result = base.GetAudioArgs();
            result.TargetVolume = CorrectedTargetValue;
            return result;
        }

        protected override float MinTargetValue { get { return AudioStatics.MinVolume; } }
        protected override float MaxTargetValue { get { return AudioStatics.MaxVolume; } }


        protected override void SetValuesToSystem(AudioArgs args)
        {
            AudioEvents.TriggerSetVolume(args);
        }

        protected override void GetValueIntoOutput(AudioArgs args)
        {
            output.Value = AudioSys.GetVolume(args);
        }
    }
}