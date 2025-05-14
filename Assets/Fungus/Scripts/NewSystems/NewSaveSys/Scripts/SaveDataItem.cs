using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// A container for a single SaveData instance, for encoding and later decoding.
    /// The data and its associated type are stored as string properties.
    /// The data would typically be a JSON string representing a saved object.
    /// </summary>
    [System.Serializable]
    public class SaveDataItem
    {
        [SerializeField] protected string dataType;
        [SerializeField] protected string data;

        public string DataType => dataType;
        public string Data => data;

        public SaveDataItem(string dataType, string data)
        {
            this.dataType = dataType;
            this.data = data;
        }

        public static SaveDataItem CreateFrom<T>(T saveData) where T : SaveData
        {
            var dataAsJson = JsonUtility.ToJson(saveData, true);
            return new SaveDataItem(typeof(T).Name, dataAsJson);
        }
    }
}