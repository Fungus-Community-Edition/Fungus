using Amanita.VScripting;
using UnityEngine;

public class VariableRefHolder : ScriptableObject
{
    [SerializeReference] public IVariable varField;
}