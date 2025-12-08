using UnityEngine;
using Amanita.VScripting;

namespace Amanita.SaveSys
{
    [System.Serializable]
    public class VariableSaveData : SaveData
    {
        [SerializeField] protected string varTypeName = string.Empty;
        [SerializeField] protected byte itemID = Variable.InvalidID;
        [SerializeField] protected string key = string.Empty;
        [SerializeField] protected string value = string.Empty;

        /// <summary>
        /// The type name of the variable this SaveData is for. NOT to be confused
        /// with TypeName, which is the type name of this SaveData instance.
        /// </summary>
        public virtual string VarTypeName
        {
            get => varTypeName;
            set => varTypeName = value;
        }

        public byte ItemId
        {
            get => itemID;
            set => itemID = value;
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
        public virtual string VarName
        {
            get => Key;
            set => Key = value;
        }

        public VariableSaveData()
        {
            this.itemID = 0;
            this.key = string.Empty;
            this.value = string.Empty;
        }

        public VariableSaveData(byte itemID = 0, string key = "", string value = "")
        {
            this.itemID = itemID;
            this.key = key;
            this.value = value;
        }

        public static readonly VariableSaveData Null = new()
        {
            itemID = Variable.InvalidID,
            key = "null",
            value = "null"
        };
    }
}