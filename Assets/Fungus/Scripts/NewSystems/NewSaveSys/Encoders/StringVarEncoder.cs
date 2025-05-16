using UnityEngine;
using Fungus;

namespace Amanita.SaveSys
{
    /// <summary>
    /// This class is responsible for encoding and decoding string data types.
    /// </summary>
    [System.Serializable]
    public class StringVarEncoder : IVarEncoder
    {
        public virtual bool CanHandle(Variable variable) =>
            variable is StringVariable;
        public virtual bool CanHandle(string typeName) =>
            typeName == nameof(StringVariable);

        public virtual string Encode(Variable variable) => ((StringVariable)variable).Value;
        public virtual void Decode(Variable variable, string data)
        {
            if (variable is StringVariable strVar)
            {
                strVar.Value = data;
            }
            else
            {
                Debug.LogError($"Variable type {variable.GetType()} is not supported for decoding in {this.GetType().Name}.");
            }
        }
    }
}