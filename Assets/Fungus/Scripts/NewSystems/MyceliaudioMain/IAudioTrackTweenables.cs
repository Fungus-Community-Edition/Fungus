using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.Myceliaudio
{
    public interface IAudioTrackTweenables
    {
        float BaseVolume { get; set; }
        GameObject GameObject { get; }
    }
}