using Amanita.VScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.Myceliaudio.VScripting
{
    [System.Serializable]
    public class FlowchartPlayAudioArgs : IPlayAudioContext
    {
        [SerializeField] protected TrackGroup trackGroup = TrackGroup.Null;
        [SerializeField] protected IntegerData track = new IntegerData(0);
        [SerializeField] protected AudioClipData introClip = new AudioClipData(null);
        [FormerlySerializedAs("clip")]
        [SerializeField] protected AudioClipData mainClip = new AudioClipData(null);
        [SerializeField] protected BooleanData loop = new BooleanData(false);
        [SerializeField] protected FloatData loopStartPoint = new FloatData(0);
        [SerializeField] protected FloatData loopEndPoint = new FloatData(0);
        [SerializeField] protected BooleanData oneShot = new BooleanData();

        public virtual TrackGroup TrackGroup { get { return trackGroup; } }
        public virtual int Track { get { return track; } }
        public virtual AudioClip MainClip { get { return mainClip.Value; } set { mainClip.Value = value; } }
        public virtual AudioClip IntroClip { get { return introClip.Value; } set { introClip.Value = value; } }
        public virtual bool Loop { get { return loop; } }
        public virtual double LoopStartPoint { get { return loopStartPoint; } }
        public virtual double LoopEndPoint { get { return loopEndPoint; } }
        public virtual bool OneShot { get { return oneShot; } }
        public virtual bool HasEndPointBeforeEndOfClip
        {
            get { return loopEndPoint > 0; }
        }

        public virtual IntegerData TrackData
        {
            get { return track; }
        }

        public virtual AudioClipData ClipData
        {
            get { return mainClip; }
        }
    }

}