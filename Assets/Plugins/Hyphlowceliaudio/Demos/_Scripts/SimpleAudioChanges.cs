using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace AtMycelia.Myceliaudio.Demos
{
    public class SimpleAudioChanges : MonoBehaviour
    {
        [SerializeField] protected AudioClip bgmToPlay, sfx;
        [SerializeField] protected float volChangeInterval = 5f;

        [Tooltip("In milliseconds")]
        [SerializeField] protected double loopPoint = 0f, endPoint = 0f;

        [Tooltip("On a scale of 0 to 100. 0 = total silence, 100 = max volume")]
        [SerializeField] protected float startingMusicVol = 50f, startingSfxVol = 25f;

        [Header("UI")]
        [SerializeField] protected Button raiseMusicVol;
        [SerializeField] protected Button lowerMusicVol, raiseSfxVol, lowerSfxVol, playSfx;
        [SerializeField] protected TMP_Text musicVolLabel, sfxVolLabel;

        protected virtual void Start()
        {
            AudioSys = AudioSystem.S;

            AudioSys.SetTrackGroupVol(TrackGroup.BGMusic, startingMusicVol);
            AudioSys.SetTrackGroupVol(TrackGroup.SoundFX, startingSfxVol);

            UpdateTextFields();

            PlayAudioArgs playShortBgm = new PlayAudioArgs()
            {
                MainClip = bgmToPlay,
                TrackGroup = TrackGroup.BGMusic,
                Loop = true,
                LoopStartPoint = loopPoint,
                LoopEndPoint = endPoint,
            };
            
            AudioSys.Play(playShortBgm);
        }

        protected virtual void UpdateTextFields()
        {
            float musicVol = AudioSys.GetTrackGroupVol(TrackGroup.BGMusic);
            musicVol = Mathf.Round(musicVol);
            musicVolLabel.text = $"Music Volume: {musicVol}%";

            float sfxVol = AudioSys.GetTrackGroupVol(TrackGroup.SoundFX);
            sfxVol = Mathf.Round(sfxVol);
            sfxVolLabel.text = $"SFX Volume: {sfxVol}%";
        }

        protected virtual void OnEnable()
        {
            raiseMusicVol.onClick.AddListener(OnRaiseMusicVolClicked);
            lowerMusicVol.onClick.AddListener(OnLowerMusicVolClicked);
            raiseSfxVol.onClick.AddListener (OnRaiseSfxVolClicked);
            lowerSfxVol.onClick.AddListener(OnLowerSfxVolClicked);

            playSfx.onClick.AddListener(OnPlaySfxClicked);
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

        protected virtual void OnRaiseSfxVolClicked()
        {
            ChangeVol(TrackGroup.SoundFX, raiseIt);
        }

        protected virtual void OnLowerSfxVolClicked()
        {
            ChangeVol(TrackGroup.SoundFX, lowerIt);
        }

        protected virtual void OnPlaySfxClicked()
        {
            playSfxArgs.MainClip = sfx;
            AudioSys.Play(playSfxArgs);
        }

        PlayAudioArgs playSfxArgs = new PlayAudioArgs()
        {
            TrackGroup = TrackGroup.SoundFX
        };

        protected virtual void OnDisable()
        {
            raiseMusicVol.onClick.RemoveListener(OnRaiseMusicVolClicked);
            lowerMusicVol.onClick.RemoveListener(OnLowerMusicVolClicked);
            raiseSfxVol.onClick.RemoveListener(OnRaiseSfxVolClicked);
            lowerSfxVol.onClick.RemoveListener(OnLowerSfxVolClicked);

            playSfx.onClick.RemoveListener(OnPlaySfxClicked);
        }
    }
}