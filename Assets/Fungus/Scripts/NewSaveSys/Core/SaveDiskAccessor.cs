using UnityEngine;

namespace Amanita.SaveSys
{
    public class SaveDiskAccessor : ScriptableObject
    {
        [Tooltip("The first part of the save files' names this works with.")]
        [SerializeField] protected string savePrefix = "saveData";
        [Tooltip("Just for flavor.")]
        [SerializeField] protected string fileExtension = "save";

        public virtual string SavePrefix => savePrefix;
        public virtual string FileExtension => fileExtension;

    }
}