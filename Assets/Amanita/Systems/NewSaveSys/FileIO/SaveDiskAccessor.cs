using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject
    {
        [Tooltip("The first part of the save files' names this works with.")]
        [SerializeField] protected string savePrefix = "saveData";
        [Tooltip("Just for flavor.")]
        [SerializeField] protected string fileExtension = "save";
        [SerializeField] protected string relativeSavePath = "/Saves/";
        [SerializeField] protected string saveNumberFormat = "D2";

        
        public virtual string SavePrefix => savePrefix;
        public virtual string FileExtension => fileExtension;
        public virtual string RelativeSavePath
        {
            get => relativeSavePath;
            set
            {
                relativeSavePath = value;

                if (string.IsNullOrEmpty(relativeSavePath))
                {
                    relativeSavePath = "/";
                }

                EnsureRelativePathInRightFormat();
            }
        }

        protected virtual void EnsureRelativePathInRightFormat()
        {
            bool isJustDash = relativeSavePath == "/" || relativeSavePath == "\\";
            if (isJustDash)
            {
                relativeSavePath = DefaultRelativeSavePath;
            }

            bool startsWithDash = relativeSavePath.StartsWith('/') || relativeSavePath.StartsWith("\\");
            if (startsWithDash)
            {
                relativeSavePath = relativeSavePath.TrimStart('/', '\\');
            }
        }

        public virtual string SaveNumberFormat => saveNumberFormat;

        public virtual string DefaultRelativeSavePath => "Saves";

        protected string fileNameFormat = "{0}_{1}.{2}";
        protected string filePathFormat = "{0}{1}"; // We expect a / or \ at the end of {0}

        public virtual string FileNameFormat => fileNameFormat;
        public virtual string FilePathFormat => filePathFormat;

        public static string ReadWriteDelimiter => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

        protected virtual string GetFolderToAccess(SaveDirectoryType directoryType)
        {
            string result = FileUtils.GetPathToFolder(directoryType, RelativeSavePath);
            return result;
        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(relativeSavePath))
            {
                relativeSavePath = DefaultRelativeSavePath;
            }
            EnsureRelativePathInRightFormat();

        }

        // For checking the validity of the save files.
        public static string CompletionMarker { get; protected set; } = "\n<!-- Amanita Save Sys: Save Completed! -->";


    }
}