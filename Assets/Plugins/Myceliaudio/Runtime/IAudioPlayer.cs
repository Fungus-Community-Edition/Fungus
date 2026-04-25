namespace AtMycelia.Myceliaudio
{
    public interface IAudioPlayer
    {
        void Play();
    }

    public interface IAudioPlayer<TArg>
    {
        void Play(TArg arg);
    }
}