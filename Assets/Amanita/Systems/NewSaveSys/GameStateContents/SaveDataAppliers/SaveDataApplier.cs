using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Amanita.SaveSys
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

        Task ApplyRange(IList<SaveData> datas);
        Task Apply(SaveData saveData);

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

        }

        public virtual int Order => order;

        public virtual bool CanApply(SaveData saveData)
        {
            return false;
        }

        public virtual async Task ApplyRange(IList<SaveData> datas)
        {
            foreach (SaveData data in datas)
            {
                if (CanApply(data))
                {
                    await Apply(data);
                }
            }

        }

        public abstract Task Apply(SaveData saveData);
    }

    /// <summary>
    /// For applying SaveData instances to the appropriate target objects.
    /// </summary>
    public abstract class SaveDataApplier<TSaveData> : SaveDataApplier
    where TSaveData : SaveData
    {
        public abstract Task Apply(TSaveData saveData);
        
        public override bool CanApply(SaveData saveData)
        {
            return saveData is TSaveData;
        }

        
    }

}