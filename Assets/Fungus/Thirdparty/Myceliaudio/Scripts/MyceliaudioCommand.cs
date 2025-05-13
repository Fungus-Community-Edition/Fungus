using UnityEngine;
using Fungus;

namespace Amanita.Myceliaudio
{
    public abstract class MyceliaudioCommand : Command
    {
        protected virtual AudioSystem AudioSys { get { return AudioSystem.S; } }
        public override Color GetButtonColor()
        {
            return audioCommandColor;
        }

        protected static Color32 audioCommandColor = new Color32(242, 209, 176, 255);
    }

}