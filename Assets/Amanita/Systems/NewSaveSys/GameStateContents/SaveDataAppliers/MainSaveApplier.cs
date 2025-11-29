using UnityEngine;
using BaseObj = System.Object;

namespace Amanita.SaveSys
{ 
    public class MainSaveApplier : MonoBehaviour, IMainSaveApplier<CompositeSaveData>
    {
        [SerializeField] protected SaveDataApplier[] subAppliers;

        public virtual void Apply(BaseObj saveData)
        {
            if (saveData is CompositeSaveData csd)
            {
                Apply(csd);
            }
            else
            {
                Debug.LogError($"Invalid save data type passed to {nameof(MainSaveApplier)}. Expected {nameof(CompositeSaveData)}, got {saveData?.GetType().Name ?? "null"}.", this);
            }
        }

        public virtual void Apply(CompositeSaveData saveData)
        {
            foreach (var applierEl in subAppliers)
            {
                
            }
        }
    }

    public interface IMainSaveApplier
    {
        void Apply(BaseObj saveData);
    }

    public interface IMainSaveApplier<T> : IMainSaveApplier
    {
        void Apply(T saveData);
    }
}