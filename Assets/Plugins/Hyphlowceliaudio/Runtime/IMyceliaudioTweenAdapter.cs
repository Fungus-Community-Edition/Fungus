using AtMycelia.AmaniTween;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Hyphlowceliaudio
{
    public interface IMyceliaudioTweenAdapter
    {
        ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration);
        ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration);
    }
}