using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Amanita.VScripting")]
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
