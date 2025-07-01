using UnityEngine;

namespace Amanita.SaveSys
{ 
    public class MainSaveApplier : MonoBehaviour
    {
        [SerializeField] protected SaveDataApplier[] subAppliers;

        public virtual void Apply(CompositeSaveData saveData)
        {
            foreach (var applierEl in subAppliers)
            {
                
            }
        }
    }
}