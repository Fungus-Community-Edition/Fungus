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
        /// For when this needs to prep fields after being deserialized.
        /// </summary>
        public virtual void OnDeserialize()
        {
        }

        public static SaveData DeserializeFrom(SerializedSaveData item)
        {
            throw new NotImplementedException("Call the concrete subclass's DeserializeFrom method instead of the abstract SaveData base class.");
        }

        public virtual string TypeName { get { return GetType().Name; } }
    }

    
}