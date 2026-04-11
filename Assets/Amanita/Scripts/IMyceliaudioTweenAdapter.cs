using AtMycelia.Amanita.Myceliaudio;

namespace AtMycelia.Hyphlow.Tweening
{
    public interface IMyceliaudioTweenAdapter
    {
        ITweenHandle FadeVolume(IAudioTrack track, float targVal, float duration);
        ITweenHandle FadeVolume01(IAudioTrack track, float targVal, float duration);
    }
}