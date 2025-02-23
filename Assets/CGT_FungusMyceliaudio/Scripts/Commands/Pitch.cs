using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;
using StringBuilder = System.Text.StringBuilder;

namespace CGT.FungusExt.Myceliaudio
{
    [CommandInfo("Audio/CGT",
        "Pitch",
        "Lets you get or set the pitch of music, sfx or voice in a given track.")]
    [AddComponentMenu("")]
    public class Pitch : SoundShifter
    {
        protected override AudioArgs GetAudioArgs()
        {
            var result = base.GetAudioArgs();
            result.TargetPitch = CorrectedTargetValue;
            return result;
        }

        protected override float MinTargetValue { get { return AudioStatics.MinPitch; } }
        protected override float MaxTargetValue { get { return AudioStatics.MaxPitch; } }

        protected override void SetValuesToSystem(AudioArgs args)
        {
            AudioEvents.TriggerSetPitch(args);
        }

        protected override void GetValueIntoOutput(AudioArgs args)
        {
            output.Value = AudioSys.GetPitch(args);
        }

    }
}