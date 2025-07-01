using System;
using UnityEngine;

namespace Amanita.Myceliaudio
{
    [System.Serializable]
    public class PlayAudioArgs : EventArgs, IPlayAudioContext, IEquatable<IPlayAudioContext>
    {
        [SerializeField] protected AudioClip _introClip;
        [SerializeField] protected AudioClip _mainClip;
        [SerializeField] protected TrackGroup _trackGroup;
        [SerializeField] protected int _track;
        [SerializeField] protected bool _loop;

        [HideInInspector]
        [SerializeField] protected string _mainClipName = string.Empty;

        public virtual AudioClip IntroClip
        {
            get { return _introClip; }
            set { _introClip = value; }
        }

        public virtual AudioClip MainClip
        {
            get { return _mainClip; }
            set
            {
                _mainClip = value;
                _mainClipName = string.Empty;

                if (value != null)
                {
                    _mainClipName = value.name;
                }
            }
        }

        public virtual TrackGroup TrackGroup
        {
            get { return _trackGroup; }
            set { _trackGroup = value; }
        }

        public virtual int Track
        {
            get { return _track; }
            set { _track = value; }
        }

        public virtual bool Loop
        {
            get { return _loop; }
            set { _loop = value; }
        }

        public virtual double LoopStartPoint
        {
            get { return _loopStartPoint; }
            set { _loopStartPoint = Math.Clamp(value, 0, double.MaxValue); }
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

        public virtual string MainClipName
        {
            get { return _mainClipName; }
        }

        public virtual bool HasEndPointBeforeEndOfClip
        {
            get { return LoopEndPoint > 0; }
        }

        public virtual bool OneShot { get; set; }

        public static PlayAudioArgs CreateCopy(PlayAudioArgs other)
        {
            PlayAudioArgs result = new PlayAudioArgs()
            {
                MainClip = other.MainClip,
                TrackGroup = other.TrackGroup,
                Track = other.Track,
                Loop = other.Loop,
                LoopStartPoint = other.LoopStartPoint,
                LoopEndPoint = other.LoopEndPoint
            };

            return result;
        }

        public static PlayAudioArgs Null => new PlayAudioArgs()
        {
            MainClip = null,
            TrackGroup = TrackGroup.Null,
            Track = 0,
            Loop = false,
            LoopStartPoint = 0,
            LoopEndPoint = 0
        };

        public virtual bool Equals(IPlayAudioContext other)
        {
            if (other == null)
            {
                return false;
            }

            return MainClip == other.MainClip &&
                   TrackGroup == other.TrackGroup &&
                   Track == other.Track &&
                   Loop == other.Loop &&
                   LoopStartPoint == other.LoopStartPoint &&
                   LoopEndPoint == other.LoopEndPoint;
        }
    }

}