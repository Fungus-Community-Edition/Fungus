using UnityEngine;

namespace Amanita.SaveSys
{ 
    public class MainSaveApplier : MonoBehaviour
    {
        [SerializeField] protected SaveDataApplier[] subAppliers;

        public virtual void Apply(AmanitaSaveData saveData)
        {
            foreach (var applierEl in subAppliers)
            {
                
            }
        }
    }
}