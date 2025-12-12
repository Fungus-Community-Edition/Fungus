using UnityEngine;
using UnityEngine.Serialization;
using Amanita.VScripting;

namespace Amanita.Myceliaudio.VScripting
{
    [CommandInfo("Myceliaudio", "MA Play Audio", "")]
    public class MA_PlayAudio : MyceliaudioCommand, ISerializationCallbackReceiver
    {
        public enum AudioPlayMode
        {
            Null,
            Play,
            Unpause
        }

        [SerializeField] protected AudioPlayMode mode = AudioPlayMode.Play;
        [SerializeField] protected FlowchartPlayAudioArgs mainPlayConfig;
        [Tooltip("If true, this Command will be skipped if the clip is already playing in the specified track.")]
        [SerializeField] protected BooleanData skipIfAlreadyPlaying = new BooleanData();

        [SerializeField] protected BooleanData useConfigSO = new BooleanData();
        [SerializeField] protected PlayAudioArgsSO configSO;

        public override void OnEnter()
        {
            base.OnEnter();
            HandlePlaying();
            HandleUnpausing();
            Continue();
        }

        protected virtual void HandlePlaying()
        {
            if (mode != AudioPlayMode.Play)
            {
                return;
            }

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
                    result = mainPlayConfig.MainClip != null;
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
                    result = mainPlayConfig.TrackGroup;
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
                    result = mainPlayConfig.Track;
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
                        result = configSO.MainClip;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = mainPlayConfig.MainClip;
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
                result = mainPlayConfig;
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

        protected virtual void HandleUnpausing()
        {
            if (mode != AudioPlayMode.Unpause)
            {
                return;
            }

            AudioSystem.S.Unpause(TrackGroup, Track);
        }

        public override string GetSummary()
        {
            string result = GetCorrectSummary();
            return result;
        }

        protected virtual string GetCorrectSummary()
        {
            string result = $"{mode} ";

            if (mode == AudioPlayMode.Play)
            {
                result += MessageForPlaying();
            }
            else if (mode == AudioPlayMode.Unpause)
            {
                result += MessageForUnpausing();
            }
            else
            {
                result = $"ERROR: {mode} is not a valid play mode.";
            }

            return result;
        }

        protected virtual string MessageForPlaying()
        {
            string result = string.Empty;
            result = $"{ClipNameForSummary()} in {TrackGroup} Tr {TrackNameForSummary()}";

            return result;
        }

        protected virtual string TrackNameForSummary()
        {
            string name = string.Empty;

            if (useConfigSO)
            {
                if (configSO != null)
                {
                    name = configSO.Track.ToString();
                }
                else
                {
                    name = "ERROR: Need config SO";
                }
            }
            else
            {
                IntegerData intData = mainPlayConfig.TrackData;
                IVariable<int> intRef = intData.integerRef;
                bool assignedVar = intRef != null;

                if (assignedVar)
                {
                    name = $"{intRef.Key}";
                }
                else
                {
                    name = $"{Track}";
                }
            }

            return name;
        }
    
        protected virtual string ClipNameForSummary()
        {
            string name = string.Empty;

            if (useConfigSO)
            {
                if (configSO != null)
                {
                    if (configSO.MainClip == null)
                    {
                        
                    }
                    else
                    { 
                        name = Clip.name;
                    }
                }
                else
                {
                    name = "ERROR: Need config SO";
                }
            }
            else
            {
                AudioClipData clipData = mainPlayConfig.ClipData;
                IVariable<AudioClip> clipRef = clipData.audioClipRef;
                bool assignedVar = clipRef != null;

                if (assignedVar)
                {
                    name = $"{clipRef.Key}";
                    if (!ValidClip)
                    {
                        name += " (clipless)";
                    }
                }
                else
                {
                    if (Clip != null)
                    {
                        name = Clip.name;
                    }
                }
            }

            return name;
        }

        protected virtual string MessageForUnpausing()
        {
            string result = $"{TrackGroup} Tr {TrackNameForSummary()}";
            return result;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            skipIfAlreadyPlaying ??= new BooleanData();
            useConfigSO ??= new BooleanData();
        }

    }

    
}