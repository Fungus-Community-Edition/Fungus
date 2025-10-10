using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject
    {
        [SerializeField] protected SaveNameSettings nameSettings;

        #region Name Settings
        public virtual string SavePrefix
        {
            get => nameSettings.Prefix;
            set => nameSettings.Prefix = value;
        }

        public virtual string FileExtension => nameSettings.FileExtension;

        public virtual string RelativeSavePath
        {
            get => nameSettings.RelativeSavePath;
            set => nameSettings.RelativeSavePath = value;
        }

        public virtual string SaveNumberFormat => nameSettings.NumberFormat;

        public virtual string DefaultRelativeSavePath => nameSettings.DefaultRelativeSavePath;

        public virtual string FileNameFormat => nameSettings.FileNameFormat;
        public virtual string FilePathFormat => nameSettings.FilePathFormat;
        #endregion

        public static string ReadWriteDelimiter => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

        protected virtual string GetFolderToAccess(SaveDirectoryType directoryType)
        {
            string result = FileUtils.GetPathToFolder(directoryType, RelativeSavePath);
            return result;
        }

        // For checking the validity of the save files.
        public static string CompletionMarker { get; protected set; } = "\n<!-- Amanita Save Sys: Save Completed! -->";
        

    }
}