using System;

namespace AtMycelia.Myceliaudio.Utils
{
    [Flags]
    public enum AudioTiming
    {
        Null = 0,
        Awake = 1 << 0,
        OnEnable = 1 << 1,
        OnDisable = 1 << 2,
        OnDestroy = 1 << 3,
    }
}