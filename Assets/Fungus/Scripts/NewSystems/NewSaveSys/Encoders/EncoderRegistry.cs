using System.Collections.Generic;
using UnityEngine;
using FungusVar = Fungus.Variable;

namespace Amanita.SaveSys
{
    public class EncoderRegistry : MonoBehaviour
    {
        private static readonly List<IVarEncoder> savers = new()
        {
            new NumericVarEncoder(),
            new VectorVarEncoder(),
            new ColorVarEncoder(),
            new StringVarEncoder(),
            /* ... */
        };

        public static IVarEncoder GetEncoder(FungusVar variable)
            => savers.Find(s => s.CanHandle(variable));

        public static IVarEncoder GetEncoder(string typeName)
            => savers.Find(s => s.CanHandle(typeName));
    }
}