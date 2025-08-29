using Amanita.VScripting;
using UnityEngine;

namespace Amanita.Tests.EditMode
{
    // Holder ScriptableObject with a VariableReference field to serialize.
    public class VarRefSO : ScriptableObject
    {
        public VariableReference reference;
    }
}