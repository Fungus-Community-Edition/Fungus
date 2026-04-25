using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Myceliaudio;

namespace AtMycelia.Hyphlowceliaudio
{
    public abstract class MyceliaudioCommand : Command
    {
        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }
        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        protected static MyceliaudioTweenAdapter MA_DefaultTweener => DefaultHyphlowceliaudioAssets.MyceliaudioTweener;
    }

}