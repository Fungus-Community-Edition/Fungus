using AtMycelia.Collections;
using UnityEngine;

namespace AtMycelia.Myceliaudio.Utils
{
    [System.Obsolete("Better to use regular QuickPlayAudio, since that lets you play audio clips on a random basis")]
    public class QuickPlayAudioRandom : MonoBehaviour
    {
        [SerializeField] protected AudioTiming _timing = AudioTiming.Awake;
        [SerializeField] protected PlayAudioArgsSO[] _audioConfigs = new PlayAudioArgsSO[] { };
        [SerializeField] protected bool _ignoreIfAlreadyPlaying;

        protected virtual void Awake()
        {
            PlayIfDesiredTimingIs(AudioTiming.Awake);
        }

        protected IPlayAudioContext _playAudio;

        protected virtual void PlayIfDesiredTimingIs(AudioTiming currentTiming)
        {
            _playAudio = _audioConfigs.GetRandom();
            bool alreadyPlaying = AudioSystem.S.GetClipPlayingAt(_playAudio.TrackGroup, _playAudio.Track) == _playAudio.MainClip;

            if (alreadyPlaying && _ignoreIfAlreadyPlaying)
            {
                return;
            }

            if ((_timing & currentTiming) > 0)
            {
                if (_playAudio.OneShot)
                {
                    AudioSystem.S.PlayOneShot(_playAudio);
                }
                else
                {
                    AudioSystem.S.Play(_playAudio);
                }
            }
        }

        protected virtual void OnEnable()
        {
            PlayIfDesiredTimingIs(AudioTiming.OnEnable);
        }

        protected virtual void OnDisable()
        {
            PlayIfDesiredTimingIs(AudioTiming.OnDisable);
        }

        protected virtual void OnDestroy()
        {
            PlayIfDesiredTimingIs(AudioTiming.OnDestroy);
        }
    }
}