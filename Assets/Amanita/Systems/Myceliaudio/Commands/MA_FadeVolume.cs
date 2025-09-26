using UnityEngine;
using Amanita.VScripting;
using Amanita.Tweening;

namespace Amanita.Myceliaudio.VScripting
{
    [CommandInfo("Myceliaudio", "MA Fade Vol", "Fades the volume of an individual track")]
    public class MA_FadeVolume : MyceliaudioCommand, ISerializationCallbackReceiver
    {
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected FloatData targetVol = new FloatData();
        [SerializeField] protected FloatData duration = new FloatData(1);
        [SerializeField] protected BooleanData waitUntilFinished = new BooleanData(true);
        [SerializeField] protected ScriptableObject fadeTween;

        protected virtual void Awake()
        {
            ValidateTweens();
        }

        protected virtual void ValidateTweens()
        {
            if (fadeTween == null)
            {
                fadeTween = AmanitaManager.DefaultTweener;
                doFade = AmanitaManager.DefaultTweener;
                return;
            }

            doFade = fadeTween as IMyceliaudioTweenAdapter;
            if (doFade == null)
            {
                Debug.Log($"Fade tweener assigned to MA_FadeVolume is not valid. It needs to implement " +
                    $"IMyceliaudioTweenAdapter. Going back to default.");
                doFade = AmanitaManager.DefaultTweener;
            }
        }

        protected IMyceliaudioTweenAdapter doFade;

        public override void OnEnter()
        {
            base.OnEnter();
            PrepFadeArgs();
            AudioSys.FadeTrackVol(fade);
            if (!waitUntilFinished)
            {
                Continue();
            }
        }

        protected virtual void PrepFadeArgs()
        {
            fade.Track = track;
            fade.TrackGroup = trackGroup;
            fade.FadeDuration = duration;
            fade.TargetValue = targetVol;
            fade.CustomFader = FadeWithTweener; // We have a fallback, so this should be fine

            if (waitUntilFinished)
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
            doFade.FadeVolume(track, args.TargetValue, args.FadeDuration)
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
            bool trackIsVar = track.integerRef != null;
            if (trackIsVar)
            {
                trackStr = track.integerRef.Key;
            }
            else
            {
                trackStr = track.Value.ToString();
            }

            string volStr;
            bool volumeIsVar = targetVol.VarRef != null;
            if (volumeIsVar)
            {
                volStr = targetVol.VarRef.Key;
            }
            else
            {
                volStr = targetVol.Value.ToString();
            }

            string durStr;
            bool durIsVol = duration.VarRef != null;
            if (durIsVol)
            {
                durStr = duration.VarRef.Key;
            }
            else
            {
                durStr = duration.Value.ToString();
            }

            string result = $"{trackGroup} Tr {trackStr} to {volStr} over {durStr} seconds";

            //Tr {track.Value} to {targetVol.Value} over {duration.Value} seconds";
            return result;
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            targetVol ??= new FloatData();
            duration ??= new FloatData(0);
            waitUntilFinished ??= new BooleanData(false);
        }

        public override void OnValidate()
        {
            base.OnValidate();
            ValidateTweens();
        }
    }
}