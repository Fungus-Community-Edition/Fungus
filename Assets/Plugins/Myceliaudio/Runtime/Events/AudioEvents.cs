using UnityEngine.Events;

namespace AtMycelia.Myceliaudio
{
    public static class AudioEvents
    {
        public static UnityAction<TrackGroup, float> TrackSetVolChanged = delegate { };
    }
}