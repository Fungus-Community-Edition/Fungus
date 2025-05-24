using UnityEngine;

namespace Amanita.SaveSys
{
    public abstract class SaveEncoder : ScriptableObject, ISaveEncoder
    {
        [SerializeField] protected int priority = 0;
        [SerializeField] protected SaveEncoder[] subEncoders = new SaveEncoder[0];

        public int Priority => priority;

        public virtual bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public virtual bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart);
        }

        public virtual object Encode()
        {
            Debug.LogError($"Encode() not implemented in {GetType().Name}.");
            return null;
        }

        public virtual SaveDataUnit Encode(object toMakeFrom = null)
        {
            Debug.LogError($"EncodeAsUnit() not implemented in {GetType().Name}.");
            return null;
        }

    }

    public abstract class SaveEncoder<TInput, TOutput> : SaveEncoder,
        ISaveEncoder<TInput, TOutput>
        where TOutput : SaveData
    {
        public abstract TOutput Encode(TInput from);

    }

}