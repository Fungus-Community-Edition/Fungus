using AtMycelia.Hyphlow;
using UnityEngine;

public class VariableRefHolder : ScriptableObject
{
    [SerializeReference] public IVariable varField;
}