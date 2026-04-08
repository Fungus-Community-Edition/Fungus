using UnityEngine;
using AtMycelia.Amanita.VScripting;

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