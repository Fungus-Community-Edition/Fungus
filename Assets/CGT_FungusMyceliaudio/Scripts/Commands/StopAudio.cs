using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

namespace CGT.FungusExt.Myceliaudio
{
    [CommandInfo("Audio/CGT",
        "StopAudio",
        "Stops the audio of the given type in the given track.")]
    [AddComponentMenu("")]
    public class StopAudio : AudioCommand
    {
        [SerializeField] protected IntegerData track = new IntegerData(0);

        public override void OnEnter()
        {
            base.OnEnter();

            AudioArgs args = GetAudioArgs();
            AudioEvents.TriggerStopAudio(args);
        }

        protected override AudioArgs GetAudioArgs()
        {
            AudioArgs args = base.GetAudioArgs();
            args.Track = track;
            args.OnComplete = CallContinueForOnComplete;

            return args;
        }

        public override string GetSummary()
        {
            return $"{audioType} Ch {track.Value}";
        }

    }
}