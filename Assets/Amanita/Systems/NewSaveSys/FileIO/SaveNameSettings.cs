using UnityEngine;

namespace Amanita.SaveSys
{
    [CreateAssetMenu(fileName = "NewSaveNameSettings", menuName = "Amanita/SaveSys/SaveNameSettings")]
    /// <summary>
    /// Settings for how save files are named and organized in directories.
    /// </summary>}
    public class SaveNameSettings : ScriptableObject
    {
        [Tooltip("The first part of the save files' names this works with.")]
        [SerializeField] protected string prefix = "saveData";
        [Tooltip("Just for flavor.")]
        [SerializeField] protected string fileExtension = "save";
        [SerializeField] protected string relativeSavePath = "/Saves/";
        [SerializeField] protected string numberFormat = "D2";

        public virtual string Prefix
        {
            get => prefix;
            set => prefix = value;
        }
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

        public virtual string NumberFormat => numberFormat;

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

        public virtual string DefaultRelativeSavePath => "Saves";

        protected string fileNameFormat = "{0}_{1}.{2}";
        protected string filePathFormat = "{0}{1}"; // We expect a / or \ at the end of {0}
        public virtual string FileNameFormat => fileNameFormat;
        public virtual string FilePathFormat => filePathFormat;

        protected virtual void OnValidate()
        {
            if (string.IsNullOrEmpty(RelativeSavePath))
            {
                RelativeSavePath = DefaultRelativeSavePath;
            }

            EnsureRelativePathInRightFormat();

            if (string.IsNullOrEmpty(Prefix))
            {
                Prefix = DefaultSavePrefix;
            }
        }

        public static string DefaultSavePrefix => "saveData";

    }
}
