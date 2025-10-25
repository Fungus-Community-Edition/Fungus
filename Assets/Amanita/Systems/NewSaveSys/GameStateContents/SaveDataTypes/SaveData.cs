using System;
using FullSerializer;

namespace Amanita.SaveSys
{
    [Serializable]
    public abstract class SaveData : ISaveData
    {
        public SaveData() { }

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

        protected static fsSerializer Serializer => AmanitaManager.DefaultSerializer;

    }

    public interface ISaveData
    {
        void OnDeserialize();
        string TypeName { get; }
    }

}