using System;
using System.Globalization;
using UnityEngine;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData
    {
        public abstract SerializedSaveData Serialized();

        /// <summary>
        /// For when this needs to prep stuff before being serialized.
        /// </summary>
        public virtual void OnDeserialize()
        {
        }

        /// <summary>
        /// Meant to be overridden by subclasses.
        /// </summary>
        public static SaveData DeserializeFrom(SerializedSaveData item)
        {
            throw new NotImplementedException("Call the concrete subclass's DeserializeFrom method instead of the abstract SaveData base class.");
        }

        public virtual string TypeName => GetType().Name;

        protected static void ValidateSerializedData(SerializedSaveData item, string expectedTypeName)
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

            if (string.IsNullOrEmpty(item.Data))
            {
                Debug.LogError($"SerializedSaveData has no data. Cannot deserialize.");
                return;
            }
        }

    }

    
}