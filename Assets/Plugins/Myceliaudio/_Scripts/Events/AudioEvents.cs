using UnityEngine.Events;

namespace Myceliaudio
{
    public static class AudioEvents
    {
        public static UnityAction<TrackGroup, float> TrackSetVolChanged = delegate { };
    }
}