namespace AtMycelia.SaveSys
{
    public interface ISaveSystemInstaller
    {
        void Init(SaveSystemInstallContext context = null);
    }

    public sealed class SaveSystemInstallContext
    {
        public SaveSystemSettings SettingsOverride { get; set; }
    }
}