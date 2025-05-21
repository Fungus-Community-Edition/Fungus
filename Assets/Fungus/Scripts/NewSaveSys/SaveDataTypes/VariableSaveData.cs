using UnityEngine;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class VariableSaveData : SaveData
    {
        [SerializeField] protected string varTypeName = string.Empty;
        [SerializeField] protected string uniqueID = string.Empty;
        [SerializeField] protected string key = string.Empty;
        [SerializeField] protected string value = string.Empty;

        public virtual string VarTypeName
        {
            get => varTypeName;
            set => varTypeName = value;
        }

        public string UniqueID
        {
            get => uniqueID;
            set => uniqueID = value;
        }

        public string Key
        {
            get => key;
            set => key = value;
        }

        public string Value
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// Alias for the Key property.
        /// </summary>
        public virtual string VarName => key;

        public VariableSaveData(string uniqueID = "", string key = "", string value = "")
        {
            this.uniqueID = uniqueID;
            this.key = key;
            this.value = value;
        }

        public override SerializedSaveData Serialized()
        {
            SerializedSaveData serializedSaveData = new()
            {
                DataTypeName = TypeName,
                Data = JsonUtility.ToJson(this, true)
            };
            return serializedSaveData;
        }

        public static readonly VariableSaveData Null = new()
        {
            uniqueID = "Null",
            key = "null",
            value = "null"
        };

        public static new VariableSaveData DeserializeFrom(SerializedSaveData item)
        {
            ValidateSerializedData(item, nameof(VariableSaveData));
            VariableSaveData data = JsonUtility.FromJson<VariableSaveData>(item.Data);
            return data;
        }
    }
}