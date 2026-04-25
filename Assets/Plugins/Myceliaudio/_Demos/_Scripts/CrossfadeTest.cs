using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace AtMycelia.Myceliaudio.Demos
{
    public class CrossfadeTest : MonoBehaviour
    {
        [SerializeField] protected AudioClip firstClip, secondClip;
        [SerializeField] protected float volChangeInterval = 5f, fadeChangeInterval = 0.5f;
        [SerializeField] protected Button raiseMusicVol, lowerMusicVol;
        [SerializeField] protected Button raiseFadeDurButton, lowerFadeDurButton;
        [SerializeField] protected Button crossfadeButton;
        [SerializeField] protected float startingVol = 5f;
        [SerializeField] protected TMP_Text musicVolLabel, fadeDurLabel;

        [SerializeField] protected float startingMusicVol = 50f, startingFadeDur = 2f;
        [SerializeField] protected float minFadeDur = 0.5f, maxFadeDur = 5f;

        protected virtual void Awake()
        {
            AudioSys = AudioSystem.S;

            AudioSys.SetTrackGroupVol(TrackGroup.BGMusic, startingMusicVol);

            // We want both clips playing at the same time, but with the second being silent
            PlayAudioArgs playFirstClip = new PlayAudioArgs()
            {
                MainClip = firstClip,
                TrackGroup = TrackGroup.BGMusic,
                Loop = true,
            };

            AudioSys.Play(playFirstClip);

            PlayAudioArgs playSecondClip = new PlayAudioArgs()
            {
                MainClip = secondClip,
                TrackGroup = TrackGroup.BGMusic,
                Loop = true,
                Track = 1,
            };

            AudioSys.Play(playSecondClip);

            AlterAudioSourceArgs setVolume = new AlterAudioSourceArgs()
            {
                TargetValue = 0,
                TrackGroup = TrackGroup.BGMusic,
                Track = 1,
            };

            AudioSys.SetTrackVol(setVolume);

            clipCurrentlyAudible = firstClip;

            currentFadeDur = startingFadeDur;
            UpdateTextFields();
        }

        protected float currentFadeDur;

        protected AudioClip clipCurrentlyAudible;

        protected virtual void UpdateTextFields()
        {
            float musicVol = AudioSys.GetTrackGroupVol(TrackGroup.BGMusic);
            musicVol = Mathf.Round(musicVol);
            musicVolLabel.text = $"Music Volume: {musicVol}%";

            // Fade dur
            float fadeDur = currentFadeDur;
            fadeDurLabel.text = $"Fade Dur: {fadeDur}s";
        }

        protected virtual void OnEnable()
        {
            raiseMusicVol.onClick.AddListener(OnRaiseMusicVolClicked);
            lowerMusicVol.onClick.AddListener(OnLowerMusicVolClicked);

            raiseFadeDurButton.onClick.AddListener(OnRaiseFadeDurClicked);
            lowerFadeDurButton.onClick.AddListener(OnLowerFadeDurClicked);

            crossfadeButton.onClick.AddListener(OnCrossfadeButtonClicked);
        }

        protected virtual void OnRaiseMusicVolClicked()
        {
            ChangeVol(TrackGroup.BGMusic, raiseIt);
        }

        protected static int raiseIt = 1;

        protected virtual void ChangeVol(TrackGroup type, float sign)
        {
            float currentVol = AudioSys.GetTrackGroupVol(type);
            currentVol += volChangeInterval * sign;
            currentVol = Mathf.Clamp(currentVol, AudioMath.MinVol, AudioMath.MaxVol);

            AudioSys.SetTrackGroupVol(type, currentVol);

            UpdateTextFields();
        }

        protected virtual AudioSystem AudioSys { get; set; }

        protected virtual void OnLowerMusicVolClicked()
        {
            ChangeVol(TrackGroup.BGMusic, lowerIt);
        }

        protected static int lowerIt = -1;

        protected virtual void OnLowerFadeDurClicked()
        {
            ChangeFadeDur(lowerIt);
            UpdateTextFields();
        }

        protected virtual void ChangeFadeDur(int sign)
        {
            currentFadeDur += fadeChangeInterval * sign;
            currentFadeDur = Mathf.Clamp(currentFadeDur, minFadeDur, maxFadeDur);
        }

        protected virtual void OnRaiseFadeDurClicked()
        {
            ChangeFadeDur(raiseIt);
            UpdateTextFields();
        }

        protected virtual void OnCrossfadeButtonClicked()
        {
            AlterAudioSourceArgs fadeOut = new AlterAudioSourceArgs()
            {
                TrackGroup = TrackGroup.BGMusic,
                TargetValue = 0,
                FadeDuration = currentFadeDur,
            };

            AlterAudioSourceArgs fadeIn = AlterAudioSourceArgs.CreateCopy(fadeOut);
            fadeIn.TargetValue = 100f; // Note that this gets scaled by the base vol of the track type
            AudioClip nextClipToBeAudible = null;

            if (clipCurrentlyAudible == firstClip)
            {
                // Then we will fade into the second
                nextClipToBeAudible = secondClip;
                fadeOut.Track = firstClipTrack;
                fadeIn.Track = secondClipTrack;
            }
            else
            {
                nextClipToBeAudible = firstClip;
                fadeOut.Track = secondClipTrack;
                fadeIn.Track = firstClipTrack;
            }

            AudioSys.FadeTrackVol(fadeOut);
            AudioSys.FadeTrackVol(fadeIn);
            clipCurrentlyAudible = nextClipToBeAudible;

            StartCoroutine(DisableCrossfadeButton(currentFadeDur));
        }

        protected static int firstClipTrack = 0, secondClipTrack = 1;

        protected virtual IEnumerator DisableCrossfadeButton(float duration)
        {
            crossfadeButton.enabled = false;

            yield return new WaitForSeconds(duration);

            crossfadeButton.enabled = true;
        }

        protected virtual void OnDisable()
        {
            raiseMusicVol.onClick.RemoveListener(OnRaiseMusicVolClicked);
            lowerMusicVol.onClick.RemoveListener(OnLowerMusicVolClicked);

            raiseFadeDurButton.onClick.RemoveListener(OnRaiseFadeDurClicked);
            lowerFadeDurButton.onClick.RemoveListener(OnLowerFadeDurClicked);

            crossfadeButton.onClick.RemoveListener(OnCrossfadeButtonClicked);
        }
    }
}