using System;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData
    {
        public abstract SaveDataUnit Serialized();

        /// <summary>
        /// For when this needs to prep stuff before being serialized.
        /// </summary>
        public virtual void OnDeserialize()
        {
        }

        /// <summary>
        /// Meant to be overridden by subclasses.
        /// </summary>
        public static SaveData DeserializeFrom(SaveDataUnit item)
        {
            throw new NotImplementedException("Call the concrete subclass's DeserializeFrom method instead of the abstract SaveData base class.");
        }

        public virtual string TypeName => GetType().Name;

        protected static void ValidateSerializedData(SaveDataUnit item, string expectedTypeName)
        {
            if (item == null)
            {
                Debug.LogError($"SerializedSaveData is null. Cannot deserialize.");
                return;
            }

            if (string.IsNullOrEmpty(item.DataTypeName))
            {
                Debug.LogError($"SerializedSaveData has no type name. Cannot deserialize.");
                return;
            }

            if (item.DataTypeName != expectedTypeName)
            {
                Debug.LogError($"SerializedSaveData is not of type {expectedTypeName}. Cannot deserialize.");
                return;
            }

            if (string.IsNullOrEmpty(item.Content))
            {
                Debug.LogError($"SerializedSaveData has no data. Cannot deserialize.");
                return;
            }
        }

    }

    public interface ISaveData
    {
        void OnDeserialize();

        string TypeName { get; }
    }

}