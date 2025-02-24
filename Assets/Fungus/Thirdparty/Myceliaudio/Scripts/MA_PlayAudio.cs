using UnityEngine;
using Myceliaudio;

namespace Fungus.Myceliaudio
{
    [CommandInfo("Myceliaudio", "MA Play Audio", "")]
    public class MA_PlayAudio : MyceliaudioCommand
    {
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected AudioClipData clip = new AudioClipData(null);
        [SerializeField] protected BooleanData loop = new BooleanData(false);
        [SerializeField] protected FloatData loopStartPoint = new FloatData(0);
        [SerializeField] protected FloatData loopEndPoint = new FloatData(0);

        public override void OnEnter()
        {
            base.OnEnter();

            if (ValidClip)
            {
                PlayAudioArgs args = GetAudioArgs();
                AudioSystem.S.Play(args);
            }
            else
            {
                PointOutClipInvalidity();
            }

            Continue();
        }

        protected virtual bool ValidClip { get { return clip.Value != null; } }

        protected virtual PlayAudioArgs GetAudioArgs()
        {
            PlayAudioArgs result = new PlayAudioArgs
            {
                Clip = clip.Value,
                Loop = loop,
                Track = track,
                TrackGroup = trackGroup
            };

            return result;
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
            string result;
            if (!ValidClip)
            {
                result = "Error: No clip given";
            }
            else
            {
                result = $"{trackGroup} Tr {track.Value} {clip.Value.name}";
            }

            return result;
        }
    }
}