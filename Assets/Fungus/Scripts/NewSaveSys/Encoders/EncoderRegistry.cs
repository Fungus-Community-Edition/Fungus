using System.Collections.Generic;
using UnityEngine;
using AmanitaVar = Amanita.Variable;

namespace Amanita.SaveSys
{
    public class EncoderRegistry : MonoBehaviour
    {
        private static readonly List<IVarEncoder> savers = new()
        {
            new NumericVarEncoder(),
            new BooleanVarEncoder(),
            new StringVarEncoder(),
            new VectorVarEncoder(),
            new ColorVarEncoder(),
            new TransformVarEncoder(),
            
            /* ... */
        };

        public static IVarEncoder GetEncoder(AmanitaVar variable)
            => savers.Find(s => s.CanHandle(variable));

        public static IVarEncoder GetEncoder(VariableSaveData saveData)
            => savers.Find(s => s.CanHandle(saveData));

        public static IVarEncoder GetEncoder(string typeName)
            => savers.Find(s => s.CanHandle(typeName));
    }
}