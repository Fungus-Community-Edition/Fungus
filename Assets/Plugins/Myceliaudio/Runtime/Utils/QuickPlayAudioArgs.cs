using System;
using UnityEngine;

namespace AtMycelia.Myceliaudio.Utils
{
    public class QuickPlayAudioArgs : EventArgs
    {
        [SerializeField] protected AudioTiming _timing = AudioTiming.Awake;
        [SerializeField] protected AudioClip _clip;
        [SerializeField] protected int _track;
        [SerializeField] protected TrackGroup _trackGroup = TrackGroup.BGMusic;
        [SerializeField] protected bool _loop;
        [Tooltip("In milliseconds")]
        [SerializeField] protected double _loopStartPoint = 0f, _loopEndPoint = 0f;
    }
}