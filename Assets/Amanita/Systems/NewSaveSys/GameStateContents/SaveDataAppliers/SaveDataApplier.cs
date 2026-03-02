using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.SaveSys
{
    public interface ISaveDataApplier
    {
        /// <summary>
        /// Gets called during SaveSystem initialization for any setup this applier needs to do.
        /// </summary>
        void PreInstallInit();

        /// <summary>
        /// Decides when this applier should be executed relative to other appliers.
        /// Lower order means it will execute sooner.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Checks if this applier can apply the given SaveData.
        /// </summary>
        bool CanApply(SaveData saveData);

        void Apply(SaveData saveData, Action onComplete);
        void ApplyRange(IList<SaveData> datas, Action onComplete);

    }

    public abstract class SaveDataApplier : ScriptableObject, ISaveDataApplier
    {
        [Tooltip("Lower order = executing sooner")]
        [SerializeField] protected int order = 0;

        /// <summary>
        /// For when there are things you want this applier to do during startup.
        /// </summary>
        public virtual void PreInstallInit()
        {
            // Nothing by default
        }

        public virtual int Order => order;

        public virtual bool CanApply(SaveData saveData)
        {
            return false;
        }

        public abstract void Apply(SaveData saveData, Action onComplete);

        public abstract void ApplyRange(IList<SaveData> datas, Action onComplete);
    }

    /// <summary>
    /// For applying SaveData instances to the appropriate target objects.
    /// </summary>
    public abstract class SaveDataApplier<TSaveData> : SaveDataApplier
    where TSaveData : SaveData
    {
        public override void ApplyRange(IList<SaveData> datas, Action onComplete)
        {
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] is TSaveData typedData)
                {
                    Apply(typedData);
                }
            }
            onComplete?.Invoke();
        }

        public abstract void Apply(TSaveData saveData);
        
        public override bool CanApply(SaveData saveData)
        {
            return saveData is TSaveData;
        }

        
    }

}