using Amanita.VScripting;
using UnityEngine;

namespace VariableOperations
{
    // Holder ScriptableObject with a VariableReference field to serialize.
    public class VarRefSO : ScriptableObject
    {
        public VariableReference reference;
    }
}