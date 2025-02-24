using System;
using UnityEngine;

namespace Myceliaudio.Utils
{
    public class QuickPlayAudio : MonoBehaviour
    {
        [Flags]
        public enum AudioTiming
        {
            Null = 0,
            Awake = 1 << 0,
            OnEnable = 1 << 1,
            OnDisable = 1 << 2,
            OnDestroy = 1 << 3,
        }

        [SerializeField] protected AudioTiming _timing = AudioTiming.Awake;
        [SerializeField] protected AudioClip _clip;
        [SerializeField] protected int _track;
        [SerializeField] protected TrackGroup _trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected bool _loop;
        [Tooltip("In milliseconds")]
        [SerializeField] protected double _loopStartPoint = 0f, _loopEndPoint = 0f;

        protected virtual void Awake()
        {
            if (_clip == null)
            {
                Debug.LogWarning($"{this.gameObject}'s PlayMusicOnStart has no clip to play.");
                return;
            }

            _playAudio = new PlayAudioArgs()
            {
                Clip = _clip,
                TrackGroup = _trackGroup,
                Loop = _loop,
                LoopStartPoint = _loopStartPoint,
                LoopEndPoint = _loopEndPoint
            };

            PlayAudioIfTimingIs(AudioTiming.Awake);

        }

        protected PlayAudioArgs _playAudio;

        protected virtual void PlayAudioIfTimingIs(AudioTiming currentTiming)
        {
            if ((_timing & currentTiming) > 0)
            {
                AudioSystem.S.Play(_playAudio);
            }
        }

        protected virtual void OnEnable()
        {
            PlayAudioIfTimingIs(AudioTiming.OnEnable);
        }

        protected virtual void OnDisable()
        {
            PlayAudioIfTimingIs(AudioTiming.OnDisable);
        }

        protected virtual void OnDestroy()
        {
            PlayAudioIfTimingIs(AudioTiming.OnDestroy);
        }
    }
}