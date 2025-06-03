using System.Collections.Generic;
using UnityEngine;

namespace Amanita.SaveSys
{
    public abstract class SaveEncoder : ScriptableObject, ISaveEncoder, ISaveEncoderHandleCheck
    {
        [SerializeField] protected int priority = 0;
        [SerializeField] protected SaveEncoder[] subEncoders = new SaveEncoder[0];

        public virtual int Priority => priority;
        public virtual bool NeedsInput => false;

        public virtual object ToMakeFrom { get; set; } = null;

        public virtual bool CanHandle(object toMakeFrom)
        {
            return CanHandle(toMakeFrom.GetType().Name);
        }

        public virtual bool CanHandle(string typeName)
        {
            return typeName == nameof(Flowchart);
        }

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

    public abstract class SaveEncoder<TInput, TOutput> : SaveEncoder,
        ISaveEncoder<TInput, TOutput>
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

        
    }

}