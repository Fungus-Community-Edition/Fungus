namespace AtMycelia.SaveSys
{
    /// <summary>
    /// A convenient place to store references to any assets that the save system needs.
    /// </summary>
    public static class DefaultSaveSysAssets
    {
        public static SaveStorageSettings SaveStorageSettings
        {
            get
            {
                return storageSettings;
            }
            set
            {
                storageSettings = value;
            }
        }
        private static SaveStorageSettings storageSettings;

        public static SaveReader SaveReader;
        public static SaveWriter SaveWriter;
        public static Decryptor Decryptor;
        public static Encryptor Encryptor;
        public static SaveSystemSettings SaveSystemSettings;
    }
}