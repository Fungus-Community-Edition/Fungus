using FullSerializer;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public abstract class SaveCodec : ScriptableObject, ISaveCodec, ISaveCodecHandleCheck
    {
        [Tooltip("Lower number = earlier processing.")]
        [SerializeField] protected int order = 0;
        [Tooltip("Sub-codecs that this one can use to decode data. This list gets sorted automatically based on the order the contents would execute in.")]
        [SerializeField] protected SaveCodec[] subCodecs = new SaveCodec[0];

        public virtual int Order => order;
        public virtual bool NeedsInput => false;

        public virtual object ToMakeFrom { get; set; } = null;

        public virtual bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public abstract bool CanHandle(string typeName);
        public abstract SaveData DecodeFrom(SaveDataUnit unit);

        public IList<SaveData> DecodeMultiFrom(IList<SaveDataUnit> units)
        {
            IList<SaveData> results = new List<SaveData>();
            for (int i = 0; i < units.Count; i++)
            {
                SaveDataUnit currentUnit = units[i];
                SaveData decodedData = DecodeFrom(currentUnit);
                if (decodedData != null)
                {
                    results.Add(decodedData);
                }
            }
            return results;
        }
        
        /// <summary>
        /// Make sure to override this, not calling the base
        /// </summary>
        public abstract SaveDataUnit EncodeToUnit();

        protected static fsSerializer Serializer => AmanitaManager.DefaultSerializer;

        protected virtual void OnValidate()
        {
            subCodecs ??= new SaveCodec[0];

            // Sort the codecs by order
            System.Array.Sort(subCodecs, (a, b) => a.Order.CompareTo(b.Order));
        }

    }

    public abstract class SaveCodec<TInput, TOutput> : SaveCodec,
        ISaveCodec<TInput, TOutput>
        where TInput: class
        where TOutput : SaveData
    {
        public virtual new TInput ToMakeFrom
        {
            get => base.ToMakeFrom as TInput;
            set => base.ToMakeFrom = value;
        }

        public abstract TOutput EncodeToSave(TInput from);

        /// <summary>
        /// Make sure to override this, not calling the base
        /// </summary>
        public abstract SaveDataUnit EncodeToUnit(TInput from);

        TOutput ISaveCodec<TInput, TOutput>.DecodeFrom(SaveDataUnit unit)
        {
            throw new System.NotImplementedException();
        }
    }

}