using UnityEngine.Events;

namespace AtMycelia.Amanita.Myceliaudio
{
    public static class AudioEvents
    {
        public static UnityAction<TrackGroup, float> TrackSetVolChanged = delegate { };
    }
}