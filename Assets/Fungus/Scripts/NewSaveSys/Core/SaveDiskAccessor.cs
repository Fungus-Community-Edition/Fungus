using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject
    {
        [Tooltip("The first part of the save files' names this works with.")]
        [SerializeField] protected string savePrefix = "saveData";
        [Tooltip("Just for flavor.")]
        [SerializeField] protected string fileExtension = "save";
        [SerializeField] protected string relativeSavePath = "Saves/";

        public virtual string SavePrefix => savePrefix;
        public virtual string FileExtension => fileExtension;
        public virtual string RelativeSavePath
        {
            get => relativeSavePath;
            set => relativeSavePath = value;
        }

        public virtual string DefaultRelativeSavePath => "Saves/";

        protected string fileNameFormat = "{0}_0{1}.{2}";
        protected string filePathFormat = "{0}/{1}";

        public virtual string FileNameFormat => fileNameFormat;
        public virtual string FilePathFormat => filePathFormat;

        public static string ReadWriteDelimiter => "\n\n<<letUsSeparateTheDataGoodSir,OrMyNameIsNotWeeweeMaximus>>\n\n";

    }
}