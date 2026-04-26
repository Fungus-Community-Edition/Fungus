using AtMycelia.AmaniTween;
using AtMycelia.AmaniTween.VScripting;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.Sys;
using AtMycelia.Myceliaudio;
using UnityEngine;

namespace AtMycelia.Hyphlowceliaudio
{
    /// <summary>
    /// Makes it so that two individual tracks in the same TrackGroup crossfade with each other,
    /// trading their individual base volumes.
    /// </summary>
    [CommandInfo("Myceliaudio",
        "Crossfade",
        "Makes it so that two individual tracks in the same TrackGroup crossfade with each other, " +
        "trading their individual base volumes.")]
    public class MA_CrossfadeTracks : BaseSimpleTweenCommand
    {
        [SerializeField] protected TrackGroup _trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected IntegerData _firstTrack = new IntegerData(0);
        [SerializeField] protected IntegerData _secondTrack = new IntegerData(1);

        public override void OnEnter()
        {
            PrepareArgs();
            base.OnEnter();
        }

        protected override void ValidateTweener()
        {
            base.ValidateTweener();
            _tweenerToUse = _tweenerSO as IMyceliaudioTweenAdapter;
            if (_tweenerToUse == null)
            {
                Debug.LogError("The provided tweener does not implement IMyceliaudioTweenAdapter." +
                    "Please provide a compatible tweener.");
            }
        }

        private void PrepareArgs()
        {
            _forFirstTrack.TrackGroup = _forSecondTrack.TrackGroup = _trackGroup;
            _forFirstTrack.FadeDuration = _forSecondTrack.FadeDuration = _duration.Value;
            
            _forFirstTrack.Track = _firstTrack.Value;
            float secondTrackBaseVol = AudioSys.GetTrackBaseVol(_trackGroup, _secondTrack.Value);
            _forFirstTrack.TargetValue = secondTrackBaseVol;
            _forFirstTrack.CustomFader = FirstFadeWithTweener;
            _forFirstTrack.OnComplete = OnOneTweenComplete;
            // ^Best only assign to one of these arg sets, given how we don't want this to 
            // call Continue twice in succession. Doesn't matter which one, so I just 
            // picked the first.

            _forSecondTrack.Track = _secondTrack.Value;
            float firstTrackBaseVol = AudioSys.GetTrackBaseVol(_trackGroup, _firstTrack.Value);
            _forSecondTrack.TargetValue = firstTrackBaseVol;
            _forSecondTrack.CustomFader = SecondFadeWithTweener;

        }

        protected AlterAudioSourceArgs _forFirstTrack = new AlterAudioSourceArgs(),
            _forSecondTrack = new AlterAudioSourceArgs();

        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }

        private void OnOneTweenComplete(AlterAudioSourceArgs args)
        {
            OnTweenComplete();
        }

        // Separate methods so we can keep track of the separate tweens.
        protected virtual void FirstFadeWithTweener(AlterAudioSourceArgs args, IAudioTrack track)
        {
            _firstTrackTween = _tweenerToUse.FadeVolume(track, args.TargetValue, args.FadeDuration)
                .SetOnComplete(() => args.OnComplete(args));
        }

        protected virtual void SecondFadeWithTweener(AlterAudioSourceArgs args, IAudioTrack track)
        {
            _secondTrackTween = _tweenerToUse.FadeVolume(track, args.TargetValue, args.FadeDuration)
                .SetOnComplete(() => args.OnComplete(args));
        }

        protected override void RegisterAllTargets()
        {
            _allTargets.Add(this);
        }

        protected override bool AreTargetsValid()
        {
            return true;
        }

        protected override ITweenHandle PrepAndExecuteTween()
        {
            _firstTrackTween?.Kill();
            _secondTrackTween?.Kill();

            float firstTrackBaseVol = AudioSys.GetTrackVol(_trackGroup, _firstTrack.Value);
            float secondTrackBaseVol = AudioSys.GetTrackVol(_trackGroup, _secondTrack.Value);

            AudioSys.FadeTrackVol(_forFirstTrack);
            AudioSys.FadeTrackVol(_forSecondTrack);
            return null;
        }

        protected MultiTweenHandle _multiTweenHandle;
        protected ITweenHandle _firstTrackTween, _secondTrackTween;
        protected IMyceliaudioTweenAdapter _tweenerToUse;

        protected override void GoWithDefaultTweener()
        {
            _tweenerSO = MA_DefaultTweener;
        }

        protected static MyceliaudioTweenAdapter MA_DefaultTweener => DefaultHyphlowceliaudioAssets.MyceliaudioTweener;

        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        public override string GetSummary()
        {
            string firstTrackStr = _firstTrack.RepresentingVar ?
                _firstTrack.VarRef.Key : 
                _firstTrack.Value.ToString();

            string secondTrackStr = _secondTrack.RepresentingVar ?
                _secondTrack.VarRef.Key :
                _secondTrack.Value.ToString();

            string durationStr = _duration.RepresentingVar ?
                _duration.VarRef.Key :
                _duration.Value.ToString();
            string result = $"{firstTrackStr} <-> {secondTrackStr} in {_trackGroup} over {durationStr}s";
            return result;
        }

    }
}