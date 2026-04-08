using UnityEngine;

namespace AtMycelia.Hyphlow
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
