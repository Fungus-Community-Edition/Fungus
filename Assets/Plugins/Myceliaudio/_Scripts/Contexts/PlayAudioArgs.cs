using System;
using UnityEngine;

namespace Myceliaudio
{
    public class PlayAudioArgs : EventArgs
    {
        public virtual AudioClip Clip { get; set; }
        public virtual TrackGroup TrackGroup { get; set; }
        public virtual int Track { get; set; }
        public virtual bool Loop { get; set; }

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
        protected double _loopStartPoint;

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
        protected double _loopEndPoint;

        public virtual bool HasLoopEndPoint
        {
            get { return LoopEndPoint > 0; }
        }

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