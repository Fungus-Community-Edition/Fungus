using System;
using FullSerializer;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData : ISaveData
    {
        public SaveData() { }

        /// <summary>
        /// For when this needs to prep stuff after being deserialized.
        /// </summary>
        public virtual void OnDeserialize()
        {
        }

        /// <summary>
        /// The name of the type of this SaveData instance. The idea is to make it easier
        /// to tell exactly what type of SaveData this is when deserializing.
        /// </summary>
        public virtual string TypeName => GetType().Name;

    }

    public interface ISaveData
    {
        void OnDeserialize();
        string TypeName { get; }
    }

}