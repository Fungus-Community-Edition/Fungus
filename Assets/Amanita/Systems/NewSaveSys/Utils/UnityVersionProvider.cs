using UnityEngine;

namespace AtMycelia.SaveSys
{
    public class UnityVersionProvider : IVersionProvider
    {
        public string GetVersion() => Application.version;
    }
}