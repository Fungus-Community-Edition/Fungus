using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace Myceliaudio
{
    public class PlayAudioMarker : Marker, INotification
    {
        [TextArea(3, 6)]
        [SerializeField] protected string notes = string.Empty;
        [SerializeField] protected PlayAudioArgs[] _argSet = new PlayAudioArgs[] { };
        
        public virtual string Notes { get { return notes; } }
        public virtual IList<PlayAudioArgs> ArgSet { get { return _argSet; } }
        public virtual string Name { get { return name; } }

        public PropertyName id => new PropertyName();
    }
}