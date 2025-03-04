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
        [Tooltip("If true, this Command will be skipped if the clip is already playing in the specified track.")]
        [SerializeField] protected BooleanData skipIfAlreadyPlaying = new BooleanData();

        public override void OnEnter()
        {
            base.OnEnter();

            if (ValidClip)
            {
                AudioClip whatIsPlayingThere = AudioSys.GetClipPlayingAt(trackGroup, track);
                bool alreadyPlayingThatClipThere = whatIsPlayingThere == clip;
                bool shouldSkip = skipIfAlreadyPlaying && alreadyPlayingThatClipThere;
                if (!shouldSkip)
                {
                    PlayAudioArgs args = GetAudioArgs();
                    AudioSystem.S.Play(args);
                }
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
            bool assignedClipVar = clip.audioClipRef != null;
            if (assignedClipVar)
            {
                // In this case, we don't want to spit out an error just because the var has nothing assigned.
                // For all we know, it could be intentional; the user might want to assign something
                // to the var during runtime but not in the editor
                result = $"{trackGroup} Tr {track.Value} {clip.audioClipRef.Key} ";

                if (!ValidClip)
                {
                    result += "(has no audio assigned)";
                }
            }
            else if (ValidClip)
            {
                result = $"{trackGroup} Tr {track.Value} {clip.Value.name}";
            }
            else
            {
                result = "Error: No clip or clip variable given";
            }

            return result;
        }
    }
}