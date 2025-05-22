using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.Overlays;
using UnityEngine;

namespace Amanita.SaveSys
{
    public interface ISaveDataApplier
    {
        /// <summary>
        /// Decides when this applier should be executed relative to other appliers.
        /// Lower order means it will execute sooner.
        /// </summary>
        int Order { get; }
        /// <summary>
        /// Checks if this applier can apply the given SaveData.
        /// </summary>
        bool CanApply(SaveData saveData);
        bool CanApply(SaveDataUnit unit);

        Task ApplyMulti(IList<SaveData> datas);
        Task Apply(SaveData saveData);

    }

    public interface ISaveDataApplier<TSaveData> : ISaveDataApplier
        where TSaveData : SaveData
    {
        Task ApplyMulti(IList<TSaveData> saveData);
        Task Apply(TSaveData saveData);
    }


    public abstract class SaveDataApplier : ScriptableObject, ISaveDataApplier
    {
        [Tooltip("Lower order = executing sooner")]
        [SerializeField] protected int order = 0;

        public virtual int Order => order;

        public virtual bool CanApply(SaveData saveData)
        {
            return false;
        }

        public abstract bool CanApply(SaveDataUnit unit);

        public virtual async Task ApplyMulti(IList<SaveData> datas)
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
    public abstract class SaveDataApplier<TSaveData> : SaveDataApplier,
        ISaveDataApplier<TSaveData>
    where TSaveData : SaveData
    {

        public virtual async Task ApplyMulti(IList<TSaveData> saveData)
        {
            foreach (TSaveData data in saveData)
            {
                if (CanApply(data))
                {
                    await Apply(data);
                }
            }
        }
        public abstract Task Apply(TSaveData saveData);
        
        public override bool CanApply(SaveData saveData)
        {
            return saveData is TSaveData;
        }

        public override bool CanApply(SaveDataUnit unit)
        {
            string typeName = typeof(TSaveData).Name;
            return unit.DataTypeName == typeName;
        }

        public Task Apply(IList<TSaveData> saveData)
        {
            throw new System.NotImplementedException();
        }
    }



}