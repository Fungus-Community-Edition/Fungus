using System;

namespace AtMycelia.SaveSys
{
    public class SaveSysDisplayName : Attribute
    {
        public SaveSysDisplayName(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; }
    }

    public class SaveSysAssetName : Attribute
    {
        public SaveSysAssetName(string assetName)
        {
            AssetName = assetName;
        }
        public string AssetName { get; }
    }
}