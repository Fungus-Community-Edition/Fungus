using UnityEngine;

namespace AtMycelia.Amanita.VScripting
{
    [AddComponentMenu("")]
    public abstract class BaseVariableProperty : Command
    {
        public enum GetSet
        {
            Get,
            Set,
        }

        public GetSet getOrSet = GetSet.Get;
    }
}
