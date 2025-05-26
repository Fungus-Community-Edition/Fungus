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

    }
}