using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For most instances of SaveData that are meant to be written to disk.
    /// Think of this as the equivalent to the old SaveDataItem class.
    /// </summary>
    [System.Serializable]
    public class SerializedSaveData
    {
        [SerializeField] protected string dataType;
        [SerializeField] protected string data;

        public string DataTypeName
        {
            get => dataType;
            set => dataType = value;
        }

        public string Data
        {
            get => data;
            set => data = value;
        }

        public SerializedSaveData(string dataType = "", string data = "")
        {
            this.dataType = dataType;
            this.data = data;
        }

    }
}