//Adapted from http://wiki.unity3d.com/index.php/EnumFlagPropertyDrawer
//placed in Amanita namespace to avoid collisions with your own

using UnityEngine;

namespace AtMycelia
{
    public class EnumFlagAttribute : PropertyAttribute
    {
        public string enumName;

        public EnumFlagAttribute() { }

        public EnumFlagAttribute(string name)
        {
            enumName = name;
        }
    }
}