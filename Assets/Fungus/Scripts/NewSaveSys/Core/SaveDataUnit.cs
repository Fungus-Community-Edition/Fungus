using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    /// <summary>
    /// For most instances of SaveData that are meant to be written to disk.
    /// Think of this as the equivalent to the old SaveDataItem class.
    /// </summary>
    [System.Serializable]
    public class SaveDataUnit : ISaveDataUnit, IEquatable<SaveDataUnit>
    {
        [SerializeField] protected string dataType;
        [SerializeField] protected string content;

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

        object ISaveDataUnit.Content { get => content; set => content = value.ToString(); }

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
        string DataTypeName { get; }
        object Content { get; set; }
    }

    public interface ISaveDataUnit<TContent> : ISaveDataUnit
    {
        new TContent Content { get; set; }
    }
}