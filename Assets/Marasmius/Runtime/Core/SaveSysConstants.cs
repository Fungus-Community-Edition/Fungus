using UnityEngine.SceneManagement;

namespace AtMycelia.SaveSys
{
    public static class SaveSysConstants 
    {
        public const string PathToSaveSysDefaultsFolder = "AtMycelia/SaveSys/Defaults"; // Relative to Resources
        public static readonly Scene DoNotLoad = default;
        public static readonly string DefaultSaveVer = "1.0.0";
    }
}