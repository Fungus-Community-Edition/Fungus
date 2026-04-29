using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Hyphlowceliaudio
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

        [SerializeField] protected AudioPlayMode _mode = AudioPlayMode.Play;
        [SerializeField] protected FlowchartPlayAudioArgs _mainPlayConfig;
        [Tooltip("If true, this Command will be skipped if the clip is already playing in the specified track.")]
        [SerializeField] protected BooleanData _skipIfAlreadyPlaying = new BooleanData();

        [SerializeField] protected BooleanData _useConfigSO = new BooleanData();
        [SerializeField] protected PlayAudioArgsSO _configSO;

        public override void OnEnter()
        {
            base.OnEnter();
            HandlePlaying();
            HandleUnpausing();
            Continue();
        }

        protected virtual void HandlePlaying()
        {
            if (_mode != AudioPlayMode.Play)
            {
                return;
            }

            if (ValidClip)
            {
                AudioClip whatIsPlayingThere = AudioSys.GetClipPlayingAt(TrackGroup, Track);
                bool alreadyPlayingThatClipThere = whatIsPlayingThere == Clip;
                bool shouldSkip = _skipIfAlreadyPlaying && alreadyPlayingThatClipThere;
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

                if (_useConfigSO)
                {
                    if (_configSO == null)
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
                    result = _mainPlayConfig.MainClip != null;
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

                if (_useConfigSO)
                {
                    if (_configSO != null)
                    {
                        result = _configSO.TrackGroup;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = _mainPlayConfig.TrackGroup;
                }

                return result;
            }
        }

        protected virtual int Track
        {
            get
            {
                int result = -1;

                if (_useConfigSO)
                {
                    if (_configSO != null)
                    {
                        result = _configSO.Track;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = _mainPlayConfig.Track;
                }

                return result;
            }
        }

        protected virtual AudioClip Clip
        {
            get
            {
                AudioClip result = null;

                if (_useConfigSO)
                {
                    if (_configSO != null)
                    {
                        result = _configSO.MainClip;
                    }
                    else
                    {
                        AlertForMissingConfigSO();
                    }
                }
                else
                {
                    result = _mainPlayConfig.MainClip;
                }

                return result;
            }
        }

        protected virtual IPlayAudioContext GetPlayAudioContext()
        {
            IPlayAudioContext result = null;

            if (_useConfigSO)
            {
                if (_configSO != null)
                {
                    return _configSO;
                }
                else
                {
                    AlertForMissingConfigSO();
                }
            }
            else
            {
                result = _mainPlayConfig;
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
            if (_mode != AudioPlayMode.Unpause)
            {
                return;
            }

            AudioSystem.S.UnPause(TrackGroup, Track);
        }

        public override string GetSummary()
        {
            string result = GetCorrectSummary();
            return result;
        }

        protected virtual string GetCorrectSummary()
        {
            string result = $"{_mode} ";

            if (_mode == AudioPlayMode.Play)
            {
                result += MessageForPlaying();
            }
            else if (_mode == AudioPlayMode.Unpause)
            {
                result += MessageForUnpausing();
            }
            else
            {
                result = $"ERROR: {_mode} is not a valid play mode.";
            }

            return result;
        }

        protected virtual string MessageForPlaying()
        {
            string result = string.Empty;
            result = $"{ClipNameForSummary()} in {TrackGroup}'s {TrackNameForSummary()}";

            return result;
        }

        protected virtual string TrackNameForSummary()
        {
            string name = string.Empty;

            if (_useConfigSO)
            {
                if (_configSO != null)
                {
                    name = _configSO.Track.ToString();
                }
                else
                {
                    name = "ERROR: Need config SO";
                }
            }
            else
            {
                IntegerData intData = _mainPlayConfig.TrackData;
                IVariable intRef = intData.VarRef;
                bool assignedVar = intRef != null;

                if (assignedVar)
                {
                    name = $"{intRef.Key}";
                }
                else
                {
                    name = $"Tr {Track}";
                }
            }

            return name;
        }
    
        protected virtual string ClipNameForSummary()
        {
            string name = string.Empty;

            if (_useConfigSO)
            {
                if (_configSO != null)
                {
                    if (_configSO.MainClip == null)
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
                AudioClipData clipData = _mainPlayConfig.ClipData;
                IVariable<AudioClip> clipRef = clipData.VarRef as IVariable<AudioClip>;
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
            string result = $"{TrackGroup} {TrackNameForSummary()}";
            return result;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            _skipIfAlreadyPlaying ??= new BooleanData();
            _useConfigSO ??= new BooleanData();
        }  


    }

}