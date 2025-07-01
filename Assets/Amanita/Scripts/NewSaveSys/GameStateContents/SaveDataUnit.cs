using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For most instances of SaveData that are meant to be written to disk.
    /// Think of this as the equivalent to the old SaveDataItem class.
    /// </summary>
    [System.Serializable]
    public class SaveDataUnit : ISaveDataUnit<string>, IEquatable<SaveDataUnit>
    {
        [SerializeField] protected string key = string.Empty;
        [SerializeField] protected string dataType;
        [SerializeField] protected string content;

        public virtual string Key
        {
            get => key;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Key), "Key cannot be null.");
                }
                key = value;
            }
        }

        public string DataTypeName
        {
            get => dataType;
            set => dataType = value;
        }

        public string Content
        {
            get => content;
            set => content = value;
        }

        object ISaveDataUnit.Content
        {
            get => content;
            set => content = value.ToString();
        }

        public SaveDataUnit(string dataType = "", string data = "")
        {
            this.dataType = dataType;
            this.content = data;
        }

        public virtual bool Equals(SaveDataUnit other)
        {
            return this.dataType == other.dataType &&
                this.content == other.content;
        }

    }

    public interface ISaveDataUnit
    {
        /// <summary>
        /// For identifying this unit in a collection thereof.
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// So you can pass this to the right codec when it's time to load state.
        /// </summary>
        string DataTypeName { get; }

        /// <summary>
        /// The state itself.
        /// </summary>
        object Content { get; set; }
    }

    public interface ISaveDataUnit<TContent> : ISaveDataUnit
    {
        new TContent Content { get; set; }
    }
}