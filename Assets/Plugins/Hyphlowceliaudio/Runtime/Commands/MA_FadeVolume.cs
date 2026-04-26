using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Myceliaudio;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlowceliaudio
{
    [CommandInfo("Myceliaudio", "MA Fade Vol", "Fades the volume of an individual track")]
    public class MA_FadeVolume : MyceliaudioCommand, ISerializationCallbackReceiver
    {
        [FormerlySerializedAs("trackGroup")]
        [SerializeField] protected TrackGroup _trackGroup = TrackGroup.BGMusic;

        [FormerlySerializedAs("track")]
        [SerializeField] protected IntegerData _track = new IntegerData(0);

        [FormerlySerializedAs("targetVol")]
        [SerializeField] protected FloatData _targetVol = new FloatData();

        [FormerlySerializedAs("duration")]
        [SerializeField] protected FloatData _duration = new FloatData(1);

        [FormerlySerializedAs("waitUntilFinished")]
        [SerializeField] protected BooleanData _waitUntilFinished = new BooleanData(true);

        [FormerlySerializedAs("fadeTween")]
        [SerializeField] protected ScriptableObject _fadeTween;

        protected virtual void Awake()
        {
            ValidateTweeer();
        }

        protected virtual void ValidateTweeer()
        {
            if (_fadeTween == null)
            {
                _fadeTween = MA_DefaultTweener;
                _tweenerToUse = MA_DefaultTweener;
                return;
            }

            _tweenerToUse = _fadeTween as IMyceliaudioTweenAdapter;
            if (_tweenerToUse == null)
            {
                Debug.Log($"Fade tweener assigned to MA_FadeVolume is not valid. It needs to implement " +
                    $"IMyceliaudioTweenAdapter. Going back to default.");
                _tweenerToUse = MA_DefaultTweener;
            }
        }

        protected IMyceliaudioTweenAdapter _tweenerToUse;

        public override void OnEnter()
        {
            base.OnEnter();
            PrepFadeArgs();
            AudioSys.FadeTrackVol(fade);
            if (!_waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void PrepFadeArgs()
        {
            fade.Track = _track;
            fade.TrackGroup = _trackGroup;
            fade.FadeDuration = _duration;
            fade.TargetValue = _targetVol;
            fade.CustomFader = FadeWithTweener; // We have a fallback, so this should be fine

            if (_waitUntilFinished)
            {
                fade.OnComplete = OnFadeComplete;
            }
            else
            {
                fade.OnComplete = delegate { };
            }
        }

        protected virtual void FadeWithTweener(AlterAudioSourceArgs args, IAudioTrack track)
        {
            _tweenerToUse.FadeVolume(track, args.TargetValue, args.FadeDuration)
                .SetOnComplete(() => args.OnComplete(args));
        }

        protected AlterAudioSourceArgs fade = new AlterAudioSourceArgs();

        protected virtual void OnFadeComplete(AlterAudioSourceArgs args)
        {
            Continue();
        }

        public override string GetSummary()
        {
            string trackStr;
            bool trackIsVar = _track.integerRef != null;
            if (trackIsVar)
            {
                trackStr = _track.integerRef.Key;
            }
            else
            {
                trackStr = _track.Value.ToString();
            }

            string volStr;
            bool volumeIsVar = _targetVol.VarRef != null;
            if (volumeIsVar)
            {
                volStr = _targetVol.VarRef.Key;
            }
            else
            {
                volStr = _targetVol.Value.ToString();
            }

            string durStr;
            bool durIsVol = _duration.VarRef != null;
            if (durIsVol)
            {
                durStr = _duration.VarRef.Key;
            }
            else
            {
                durStr = _duration.Value.ToString();
            }

            string result = $"{_trackGroup} Tr {trackStr} to {volStr} over {durStr} seconds";

            //Tr {track.Value} to {targetVol.Value} over {duration.Value} seconds";
            return result;
        }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            _targetVol ??= new FloatData();
            _duration ??= new FloatData(0);
            _waitUntilFinished ??= new BooleanData(false);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            ValidateTweeer();
        }
    }
}