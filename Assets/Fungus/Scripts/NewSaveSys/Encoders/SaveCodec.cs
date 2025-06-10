using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public abstract class SaveCodec : ScriptableObject, ISaveCodec, ISaveCodecHandleCheck
    {
        [SerializeField] protected int priority = 0;
        [SerializeField] protected SaveCodec[] subCodecs = new SaveCodec[0];

        public virtual int Order => priority;
        public virtual bool NeedsInput => false;

        public virtual object ToMakeFrom { get; set; } = null;

        public virtual bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public abstract bool CanHandle(string typeName);
        public abstract SaveData DecodeFrom(SaveDataUnit unit);

        /// <summary>
        /// Make sure to override this, not calling the base
        /// </summary>
        public abstract SaveDataUnit EncodeToUnit();

        /// <summary>
        /// Finds all available data units and encodes them into a list. If none are 
        /// found, the list will be empty.
        /// </summary>
        /// <remarks>The searching and encoding processes depend on the
        /// implementation in derived classes.</remarks>
        public abstract IList<SaveDataUnit> FindAndEncodeAll();

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