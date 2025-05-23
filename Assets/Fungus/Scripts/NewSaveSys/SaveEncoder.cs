using UnityEngine;

namespace Amanita.SaveSys
{
    public abstract class SaveEncoder<TSaveDataOutput, TMakeFrom> : ScriptableObject,
        ISaveEncoder<TSaveDataOutput, TMakeFrom>
        where TSaveDataOutput : SaveData
    {
        [SerializeField] protected int priority = 0;
        public int Priority => priority;

        public abstract TSaveDataOutput Encode(TMakeFrom from);

    }

}