namespace CGT.FungusExt.Myceliaudio.Internal
{
    /// <summary>
    /// Since we don't want client code to worry about certain fields that
    /// FungusAudioSources need access to without making them
    /// members of that class.
    /// </summary>
    public class InternalAudioArgs : AudioArgs
    {
        public virtual float StartingVolume { get; set; } = 0;
        public virtual float StartingPitch { get; set; } = 0;
        public virtual new InternalAudioHandler OnComplete { get; set; } =
            (InternalAudioArgs args) => { };

        public static new InternalAudioArgs CreateCopy(AudioArgs source)
        {
            InternalAudioArgs theCopy = new InternalAudioArgs();

            theCopy.Clip = source.Clip;
            theCopy.FadeDuration = source.FadeDuration;
            theCopy.TargetPitch = source.TargetPitch;
            theCopy.Loop = source.Loop;
            theCopy.AtTime = source.AtTime;
            theCopy.OnComplete = (InternalAudioArgs args) => { source.OnComplete(args); };

            return theCopy;
        }

        public static InternalAudioArgs CreateCopy(InternalAudioArgs source)
        {
            InternalAudioArgs theCopy = CreateCopy(source as AudioArgs);

            theCopy.StartingPitch = source.StartingPitch;
            theCopy.StartingVolume = source.StartingVolume;

            return theCopy;
        }
    }
}