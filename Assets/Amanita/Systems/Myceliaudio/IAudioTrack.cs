using UnityEngine;

namespace Amanita.Myceliaudio
{
    public interface IAudioTrack
    {
        float BaseVolume { get; set; }
        GameObject GameObject { get; }
    }
}