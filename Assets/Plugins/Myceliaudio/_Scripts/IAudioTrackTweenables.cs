using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Myceliaudio
{
    public interface IAudioTrackTweenables
    {
        float BaseVolume { get; set; }
        GameObject GameObject { get; }
    }
}