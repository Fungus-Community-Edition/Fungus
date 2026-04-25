using UnityEngine;

namespace AtMycelia.Myceliaudio
{
    public interface IAudioTrack
    {
        float BaseVolume { get; set; }
        GameObject GameObject { get; }
    }
}