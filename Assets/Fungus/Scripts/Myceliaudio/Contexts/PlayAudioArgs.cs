using System;
using UnityEngine;

namespace Amanita.Myceliaudio
{
    [System.Serializable]
    public class PlayAudioArgs : EventArgs, IPlayAudioContext
    {
        [field: SerializeField] public virtual AudioClip Clip { get; set; }
        [field: SerializeField] public virtual TrackGroup TrackGroup { get; set; }
        [field: SerializeField] public virtual int Track { get; set; }
        [field: SerializeField] public virtual bool Loop { get; set; }

        public virtual double LoopStartPoint
        {
            get
            {
                return _loopStartPoint;
            }
            set
            {
                _loopStartPoint = Math.Clamp(value, 0, double.MaxValue);
            }
        }
        [SerializeField] protected double _loopStartPoint;

        public virtual double LoopEndPoint
        {
            get
            {
                return _loopEndPoint;
            }
            set
            {
                _loopEndPoint = Math.Clamp(value, 0, double.MaxValue);
            }
        }
        [SerializeField] protected double _loopEndPoint;

        public virtual bool HasEndPointBeforeEndOfClip
        {
            get { return LoopEndPoint > 0; }
        }

        [field: SerializeField] public virtual bool OneShot { get; set; }

        public static PlayAudioArgs CreateCopy(PlayAudioArgs other)
        {
            PlayAudioArgs result = new PlayAudioArgs()
            {
                Clip = other.Clip,
                TrackGroup = other.TrackGroup,
                Track = other.Track,
                Loop = other.Loop,
                LoopStartPoint = other.LoopStartPoint,
                LoopEndPoint = other.LoopEndPoint
            };

            return result;
        }

    }
}