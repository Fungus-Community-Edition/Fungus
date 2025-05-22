using UnityEngine;
using System.Collections.Generic;

namespace Amanita.SaveSys
{
    public abstract class SaveDataApplier : ScriptableObject
    {
        [Tooltip("Higher priority = executing sooner.")]
        [SerializeField] protected int priority = 0;

        public virtual int Priority => priority;

        public virtual bool CanApply(SaveData saveData)
        {
            return false;
        }
    }

    /// <summary>
    /// For applying SaveData instances to the appropriate target objects.
    /// </summary>
    public abstract class SaveDataApplier<TSaveData> : SaveDataApplier,
        ISaveDataApplier<TSaveData>
    where TSaveData : SaveData
    {
        
        public abstract void Apply(TSaveData saveData);
        public abstract void Apply(IList<TSaveData> saveData);
        public override bool CanApply(SaveData saveData)
        {
            return saveData is TSaveData;
        }
    }



    public interface ISaveDataApplier<TSaveData>
    where TSaveData : SaveData
    {
        // Applies the SaveData to the target game object
        void Apply(IList<TSaveData> saveData);
    }

}