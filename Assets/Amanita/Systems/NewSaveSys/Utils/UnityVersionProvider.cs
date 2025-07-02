using UnityEngine;

namespace Amanita.SaveSys
{
    public class UnityVersionProvider : IVersionProvider
    {
        public string GetVersion() => Application.version;
    }
}