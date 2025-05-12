using UnityEngine;
using Fungus;
using UnityEngine.Serialization;

namespace Amanita.Myceliaudio
{
    [CommandInfo("Myceliaudio", "MA Play Audio", "")]
    public class MA_PlayAudio : MyceliaudioCommand
    {
        [FormerlySerializedAs("mainConfig")]
        [SerializeField] protected FlowchartPlayAudioArgs mainCommandConfig;
        [Tooltip("If true, this Command will be skipped if the clip is already playing in the specified track.")]
        [SerializeField] protected BooleanData skipIfAlreadyPlaying = new BooleanData();

        [SerializeField] protected BooleanData useConfigSO = new BooleanData();
        [SerializeField] protected PlayAudioArgsSO configSO;

        public override void OnEnter()
        {
            base.OnEnter();

            if (ValidClip)
            {
                AudioClip whatIsPlayingThere = AudioSys.GetClipPlayingAt(TrackGroup, Track);
                bool alreadyPlayingThatClipThere = whatIsPlayingThere == Clip;
                bool shouldSkip = skipIfAlreadyPlaying && alreadyPlayingThatClipThere;
                if (!shouldSkip)
                {
                    IPlayAudioContext args = GetPlayAudioContext();
                    AudioSystem.S.Play(args);
                }
            }
            else
            {
                PointOutClipInvalidity();
            }

            Continue();
        }

        protected virtual bool ValidClip
        {
            get
            {
                bool result;

                if (useConfigSO)
                {
                    if (configSO == null)
                    {
                        AlertForMissingConfigSO();
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                else
                {
                    result = mainCommandConfig.Clip != null;
                }

                return result;
            }
        }

        protected static void AlertForMissingConfigSO()
        {
            Debug.LogError(missingUseConfigSOMessage);
        }

        protected static string missingUseConfigSOMessage = $"Needs a config SO. If you'd rather not use one for this, then set useConfigSO to false.";

        protected virtual TrackGroup TrackGroup
        {
            get
            {
                TrackGroup result = TrackGroup.Null;

                if (useConfigSO)
                {
                    if (configSO != null)
                    {
                        result = configSO.TrackGroup;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = mainCommandConfig.TrackGroup;
                }

                return result;
            }
        }

        protected virtual int Track
        {
            get
            {
                int result = -1;

                if (useConfigSO)
                {
                    if (configSO != null)
                    {
                        result = configSO.Track;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = mainCommandConfig.Track;
                }

                return result;
            }
        }

        protected virtual AudioClip Clip
        {
            get
            {
                AudioClip result = null;

                if (useConfigSO)
                {
                    if (configSO != null)
                    {
                        result = configSO.Clip;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = mainCommandConfig.Clip;
                }

                return result;
            }
        }

        protected virtual IPlayAudioContext GetPlayAudioContext()
        {
            IPlayAudioContext result = null;

            if (useConfigSO)
            {
                if (configSO != null)
                {
                    return configSO;
                }
                else
                {
                    AlertForMissingConfigSO();
                }
            }
            else
            {
                result = mainCommandConfig;
            }

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

            if (useConfigSO)
            {
                if (configSO != null)
                {
                    result = $"{TrackGroup} Tr {Track} {Clip.name} ";
                }
                else
                {
                    result = "ERROR: Needs config object";
                }
            }
            else
            {
                AudioClipData clipData = mainCommandConfig.ClipData;
                AudioClipVariable clipRef = clipData.audioClipRef;
                bool assignedClipVar = clipRef != null;

                if (assignedClipVar)
                {
                    // In this case, we don't want to spit out an error just because the var has nothing
                    // assigned. For all we know, it could be intentional; the user might want to assign
                    // something to the var during runtime but not in the editor.
                    result = $"{TrackGroup} Tr {Track} {clipRef.Key} ";

                    if (!ValidClip)
                    {
                        result += "(clipless)";
                    }
                }
                else if (ValidClip)
                {
                    result = $"{trackGroup} Tr {Track} {Clip.name}";
                }
                else
                {
                    result = "Error: No clip or clip variable given";
                }
            }
            
            return result;
        }
    }

    [System.Serializable]
    public class FlowchartPlayAudioArgs : IPlayAudioContext
    {
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.Null;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected AudioClipData clip = new AudioClipData(null);
        [SerializeField] protected BooleanData loop = new BooleanData(false);
        [SerializeField] protected FloatData loopStartPoint = new FloatData(0);
        [SerializeField] protected FloatData loopEndPoint = new FloatData(0);
        [SerializeField] protected BooleanData oneShot = new BooleanData();

        public virtual TrackGroup TrackGroup { get { return trackGroup; } }
        public virtual int Track { get { return track; } }
        public virtual AudioClip Clip { get { return clip; } set { clip.Value = value; } }
        public virtual bool Loop { get { return loop; } }
        public virtual double LoopStartPoint {  get { return loopStartPoint; } }
        public virtual double LoopEndPoint { get { return loopEndPoint; } }
        public virtual bool OneShot { get { return oneShot; } }
        public virtual bool HasEndPointBeforeEndOfClip
        {
            get { return loopEndPoint > 0; }
        }

        public virtual IntegerData TrackData
        {
            get { return track; }
        }

        public virtual AudioClipData ClipData
        {
            get { return clip; }
        }
    }
}