using UnityEngine;

namespace AtMycelia.Amanita.Myceliaudio
{
    public interface IAudioTrack
    {
        float BaseVolume { get; set; }
        GameObject GameObject { get; }
    }
}