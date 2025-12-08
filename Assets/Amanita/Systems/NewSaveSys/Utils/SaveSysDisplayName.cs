using System;

namespace Amanita.SaveSys
{
    public class SaveSysDisplayName : Attribute
    {
        public SaveSysDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }
}