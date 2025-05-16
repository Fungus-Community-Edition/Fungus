using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For most instances of SaveData that are meant to be written to disk.
    /// </summary>
    [System.Serializable]
    public class SerializedSaveData
    {
        [SerializeField] protected string dataType;
        [SerializeField] protected string data;

        public string DataType => dataType;
        public string Data => data;

        public SerializedSaveData(string dataType, string data)
        {
            this.dataType = dataType;
            this.data = data;
        }

        public static SerializedSaveData CreateFrom<T>(T saveData) where T : SaveData
        {
            var dataAsJson = JsonUtility.ToJson(saveData, true);
            return new SerializedSaveData(typeof(T).Name, dataAsJson);
        }
    }
}