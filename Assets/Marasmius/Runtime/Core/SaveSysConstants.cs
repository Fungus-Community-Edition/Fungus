using UnityEngine.SceneManagement;

namespace AtMycelia.SaveSys
{
    public static class SaveSysConstants 
    {
        public const string PathToSaveSysDefaultsFolder = "AtMycelia/SaveSys/Defaults"; // Relative to Resources
        public static readonly Scene DoNotLoad = default;
        public static readonly string DefaultSaveVer = "1.0.0";
        public static readonly string PathToSaveSysSettings = "AtMycelia/SaveSys/SaveSystemSettings"; // Relative to Resources

        public const string PathToDefaultSaveStorageSettings = "SaveSys/Defaults/DefaultSaveStorageSettings";
        public const string PathToDefaultEncryptor = "SaveSys/Defaults/DefaultEncryptor";

        public const string PathToDefaultDecryptor = "SaveSys/Defaults/DefaultDecryptor";
        public const string PathToDefaultSaveReader = "SaveSys/Defaults/DefaultSaveReader";
        public const string PathToDefaultSaveWriter = "SaveSys/Defaults/DefaultSaveWriter";

        public const string SaveNameVarName = "saveName",
            SaveNamePrefixVarName = "saveNamePrefix",
            SaveNameSuffixVarName = "saveNameSuffix";
    }
}