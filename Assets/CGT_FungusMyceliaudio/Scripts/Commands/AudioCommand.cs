using UnityEngine;
using Fungus;

namespace CGT.FungusExt.Myceliaudio
{
    public abstract class AudioCommand : Command
    {
        [SerializeField] protected AudioType audioType;

        protected virtual void Awake()
        {
            AudioSys.EnsureExists();
            // We want to set up a foundation so it's easy for subclasses to access
            // basic parts of the system so they don't have to set up (as much of)
            // that access themselves
        }

        protected AudioSys AudioSys { get { return AudioSys.S; } }

        protected virtual AudioArgs GetAudioArgs()
        {
            AudioArgs args = new AudioArgs();
            args.AudioType = audioType;
            args.OnComplete = CallContinueForOnComplete;
            return args;
        }

        protected virtual void CallContinueForOnComplete(AudioArgs args)
        {
            Continue();
        }

        protected virtual void FillerOnComplete(AudioArgs args) { }

        public override Color GetButtonColor()
        {
            return audioCommandColor;
        }

        protected static Color32 audioCommandColor = new Color32(242, 209, 176, 255);
    }

}