using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Hyphlowceliaudio
{
    [CommandInfo("Myceliaudio",
        "MA GetSet Vol",
        "Lets you get or set the volume of an individual track or group thereof. We work " +
        "with a scale of 0 for silent and 100 for max.")]
    public class MA_TrackVolume : MyceliaudioCommand, ISerializationCallbackReceiver
    {
        [SerializeField] protected TrackGroup _trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected GetOrSet _operation = GetOrSet.Set;
        [SerializeField] protected IntegerData _track = new IntegerData(0);
        [SerializeField] protected FloatData _targetVol = new FloatData(0);
        [SerializeField] protected TrackSelection _trackSelection = TrackSelection.Group;
        [ContentTypeConstraint(typeof(float), typeof(int), typeof(double))]
        [SerializeField] protected VariableReference _outputVar;

        public override void OnEnter()
        {
            base.OnEnter();

            switch (_operation)
            {
                case GetOrSet.Set:
                    HandleSetting();
                    break;
                case GetOrSet.Get:
                    HandleGetting();
                    break;
                default:
                    string errorMessage = $"Cannot set or get track volume when the operation is {_operation}";
                    Debug.LogError(errorMessage);
                    break;
            }

            Continue();
        }

        protected virtual void HandleSetting()
        {
            argsForIndiv.TargetValue = _targetVol;
            argsForIndiv.Track = _track;
            argsForIndiv.TrackGroup = _trackGroup;

            if (_trackSelection == TrackSelection.Indiv)
            {
                AudioSys.SetTrackVol(argsForIndiv);
            }
            if (_trackSelection == TrackSelection.Group)
            {
                AudioSys.SetTrackGroupVol(_trackGroup, _targetVol);
            }
        }

        protected AlterAudioSourceArgs argsForIndiv = new AlterAudioSourceArgs();

        protected virtual void HandleGetting()
        {
            float valueToFetch = 0;

            if (_trackSelection == TrackSelection.Indiv)
            {
                valueToFetch = AudioSys.GetTrackVol(_trackGroup, _track);
            }
            if (_trackSelection == TrackSelection.Group)
            {
                valueToFetch = AudioSys.GetTrackGroupVol(_trackGroup);
            }

            valueToFetch = Mathf.Round(valueToFetch);
            // ^Why round? Because it might've been set to some fractional value for all we know
            _outputVar.SetValue(valueToFetch);
        }

        public override string GetSummary()
        {
            string result = $"{_operation} "; //from/to {trackGroup} {trackSelection}";

            if (_operation == GetOrSet.Get)
            {
                result = SummaryForGetting();
            }
            else if (_operation == GetOrSet.Set)
            {
                result = SummaryForSetting();
            }

            return result;
        }

        protected virtual string SummaryForGetting()
        {
            string result = string.Empty;
            if (_outputVar == null)
            {
                result = "ERROR: Trying to Get val with no var to put it in";
            }
            else
            {
                result = $"{_operation} from {_trackGroup}";

                if (_trackSelection == TrackSelection.Indiv)
                {
                    string trackString = string.Empty;
                    IVariable trackVar = _track.VarRef;
                    if (trackVar != null)
                    {
                        trackString = trackVar.Key;
                    }
                    else
                    {
                        trackString = $"Tr {_track.Value.ToString()}";
                    }

                    result += $"'s {trackString}";
                }

                if (_outputVar != null && _outputVar.Variable != null)
                {
                    result += $", put into {_outputVar.VarKey}";
                }
            }

            return result;
        }

        protected virtual string SummaryForSetting()
        {
            string result = $"{_operation} {_trackGroup} ";
            string trackString = string.Empty;
            if (_trackSelection == TrackSelection.Indiv)
            {
                IVariable trackVar = _track.VarRef;
                if (trackVar != null)
                {
                    trackString = trackVar.Key;
                }
                else
                {
                    trackString = $"Tr {_track.Value.ToString()}";
                }

                result += $"'s {trackString}'s vol to ";
                //result += $"Tr {track} vol to ";
            }
            else if (_trackSelection == TrackSelection.Group)
            {
                result += $"'s vol to ";
            }

            if (_targetVol.VarRef == null)
            {
                result += _targetVol.Value;
            }
            else
            {
                result += _targetVol.VarRef.Key;
            }

            return result;
        }
    }
}