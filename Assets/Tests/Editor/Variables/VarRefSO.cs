using AtMycelia.Hyphlow;
using UnityEngine;

namespace VScriptingTests.VariableOperations
{
    // Holder ScriptableObject with a VariableReference field to serialize.
    public class VarRefSO : ScriptableObject
    {
        public VariableReference reference;
    }
}