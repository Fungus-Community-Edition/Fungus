using UnityEngine;
using AtMycelia.Hyphlow;

namespace AtMycelia.Amanita.Myceliaudio.VScripting
{
    public abstract class MyceliaudioCommand : Command
    {
        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }
        public override Color GetButtonColor()
        {
            return CommandColors.Audio;
        }
    }

}