using UnityEngine;
using Amanita.VScripting;

namespace Amanita.Myceliaudio.VScripting
{
    [CommandInfo("Myceliaudio",
        "MA GetSet Vol",
        "Lets you get or set the volume of an individual track or group thereof. We work with a scale of 0 for silent and 100 for max.")]
    public class MA_TrackVolume : MyceliaudioCommand, ISerializationCallbackReceiver
    {
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected GetOrSet operation = GetOrSet.Set;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected FloatData targetVol = new FloatData();
        [SerializeField] protected TrackSelection trackSelection = TrackSelection.Group;
        [VariableProperty(typeof(FloatVariable))]
        [SerializeField] protected FloatVariable outputVar;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            targetVol ??= new FloatData();
            //
        }

        public override void OnEnter()
        {
            base.OnEnter();

            switch (operation)
            {
                case GetOrSet.Set:
                    HandleSetting();
                    break;
                case GetOrSet.Get:
                    HandleGetting();
                    break;
                default:
                    string errorMessage = $"Cannot set or get track volume when the operation is {operation}";
                    Debug.LogError(errorMessage);
                    break;
            }

            Continue();
        }

        protected virtual void HandleSetting()
        {
            argsForIndiv.TargetValue = targetVol;
            argsForIndiv.Track = track;
            argsForIndiv.TrackGroup = trackGroup;

            if (trackSelection == TrackSelection.Indiv)
            {
                AudioSys.SetTrackVol(argsForIndiv);
            }
            if (trackSelection == TrackSelection.Group)
            {
                AudioSys.SetTrackGroupVol(trackGroup, targetVol);
            }
        }

        protected AlterAudioSourceArgs argsForIndiv = new AlterAudioSourceArgs();

        protected virtual void HandleGetting()
        {
            float valueToFetch = 0;

            if (trackSelection == TrackSelection.Indiv)
            {
                valueToFetch = AudioSys.GetTrackVol(trackGroup, track);
            }
            if (trackSelection == TrackSelection.Group)
            {
                valueToFetch = AudioSys.GetTrackGroupVol(trackGroup);
            }

            valueToFetch = Mathf.Round(valueToFetch);
            // ^Why round? Because it might've been set to some fractional value for all we know
            outputVar.Value = valueToFetch;
        }

        public override string GetSummary()
        {
            string result = $"{operation} "; //from/to {trackGroup} {trackSelection}";

            if (operation == GetOrSet.Get)
            {
                result = SummaryForGetting();
            }
            else if (operation == GetOrSet.Set)
            {
                result = SummaryForSetting();
            }

            return result;
        }

        protected virtual string SummaryForGetting()
        {
            string result = string.Empty;
            if (outputVar == null)
            {
                result = "ERROR: Trying to Get val with no var to put it in";
            }
            else
            {
                result = $"{operation} from {trackGroup}";

                if (trackSelection == TrackSelection.Indiv)
                {
                    string trackString = string.Empty;
                    IVariable<int> trackVar = track.integerRef;
                    if (trackVar != null)
                    {
                        trackString = trackVar.Key;
                    }
                    else
                    {
                        trackString = track.Value.ToString();
                    }

                    result += $"'s Tr {trackString} ";
                }

                if (outputVar != null)
                {
                    result += $", put into {outputVar.Key}";
                }
            }

            return result;
        }

        protected virtual string SummaryForSetting()
        {
            string result = $"{operation} {trackGroup}";
            string trackString = string.Empty;
            if (trackSelection == TrackSelection.Indiv)
            {
                IVariable<int> trackVar = track.integerRef;
                if (trackVar != null)
                {
                    trackString = trackVar.Key;
                }
                else
                {
                    trackString = track.Value.ToString();
                }

                result += $"Tr {trackString}'s vol to ";
                //result += $"Tr {track} vol to ";
            }
            else if (trackSelection == TrackSelection.Group)
            {
                result += $"'s vol to ";
            }

            if (targetVol.floatRef == null)
            {
                result += targetVol.Value;
            }
            else
            {
                result += targetVol.floatRef.Key;
            }

            return result;
        }
    }
}