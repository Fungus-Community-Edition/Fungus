using UnityEngine;

namespace Amanita.SaveSys
{
    public class MainSaveEncoder : MonoBehaviour, ISaveEncoder<AmanitaSaveData>
    {
        [SerializeField] protected int priority = 0;

        public virtual int Priority => priority;

        [SerializeField] protected ScriptableObject subEncoders;

        public virtual AmanitaSaveData Encode()
        {
            // 
            throw new System.NotImplementedException();
        }
    }
}