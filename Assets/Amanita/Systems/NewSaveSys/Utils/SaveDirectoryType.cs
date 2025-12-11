namespace Amanita.SaveSys
{
    public enum SaveDirectoryType
    {
        Null,
        DataPath, // Same folder as the exe, apk, etc
        PersistentDataPath, // OS-dependent folder. Overall safest option
        InTheBalls // Semantically the same as DataPath
    }
}