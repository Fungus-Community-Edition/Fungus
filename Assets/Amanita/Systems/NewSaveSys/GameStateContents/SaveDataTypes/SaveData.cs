using System;
using UnityEngine;
using FullSerializer;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData : ISaveData
    {
        public SaveData() { }

        public abstract SaveDataUnit Serialized();

        /// <summary>
        /// For when this needs to prep stuff before being serialized.
        /// </summary>
        public virtual void OnDeserialize()
        {
        }

        /// <summary>
        /// The name of the type of this SaveData instance. The idea is to make it easier
        /// to tell exactly what type of SaveData this is when deserializing.
        /// </summary>
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

        protected static fsSerializer Serializer => AmanitaManager.DefaultSerializer;

    }

    public interface ISaveData
    {
        void OnDeserialize();
        string TypeName { get; }
    }

}