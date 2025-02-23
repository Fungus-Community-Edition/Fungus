using UnityEngine;
using Fungus;
using StringBuilder = System.Text.StringBuilder;

namespace CGT.FungusExt.Myceliaudio
{
    [CommandInfo("Audio/CGT",
        "PlayAudio",
        "Plays an audio clip in the given track.")]
    [AddComponentMenu("")]
    public class PlayAudio : AudioCommand
    {
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected AudioClipData clip = new AudioClipData(null);
        [SerializeField] protected BooleanData loop = new BooleanData(false);

        public override void OnEnter()
        {
            base.OnEnter();

            if (ValidClip)
            {
                AudioArgs args = GetAudioArgs();
                AudioEvents.TriggerPlayAudio(args);
            }
            else
            {
                PointOutClipInvalidity();
            }

            Continue();
        }

        protected virtual bool ValidClip { get { return clip.Value != null; } }

        protected override AudioArgs GetAudioArgs()
        {
            AudioArgs args = base.GetAudioArgs();
            args.Clip = clip.Value;
            args.Loop = loop;
            args.Track = track;

            return args;
        }

        protected virtual void PointOutClipInvalidity()
        {
            // To make debugging easier for the user
            string flowchartName = gameObject.name;
            string blockName = ParentBlock.BlockName;
            int index = CommandIndex;

            string errorMessage = $"PlayAudio Command invalid in Flowchart in GameObject {flowchartName}, Block {blockName}, Index {index}. Reason: No valid AudioClip assigned";
            Debug.LogWarning(errorMessage);
        }

        public override string GetSummary()
        {
            if (!ValidClip)
                return "Error: No clip given";

            forSummary.Clear();
            forSummary.Append($"{audioType} Tr {track.Value} {clip.Value.name} ");

            return forSummary.ToString();
        }

        protected StringBuilder forSummary = new StringBuilder();

    }
}