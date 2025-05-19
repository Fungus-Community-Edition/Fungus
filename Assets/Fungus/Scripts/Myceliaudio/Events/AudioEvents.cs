using UnityEngine.Events;

namespace Amanita.Myceliaudio
{
    public static class AudioEvents
    {
        public static UnityAction<TrackGroup, float> TrackSetVolChanged = delegate { };
    }
}