using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.Tweening;

namespace AtMycelia.Amanita.Myceliaudio.VScripting
{
    public abstract class MyceliaudioCommand : Command
    {
        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }
        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }

        protected static MyceliaudioTweenAdapter MA_DefaultTweener => DefaultAmanitaAssets.MyceliaudioTweener;
    }

}