namespace Amanita.VScripting
{
    /// <summary>
    /// A simple struct wrapping a reference to a Fungus Variable or Muscariable.
    /// </summary>
    [System.Serializable]
    public struct VariableReference
    {
        // New: managed-reference to modern variables
        [UnityEngine.SerializeReference] public IVariable variable;

        // Legacy fallback to keep old behavior/assets working where needed
        public Variable legacyVariable;

        public T Get<T>()
        {
            // Prefer modern variable
            if (variable is IVariable<T> typed)
                return typed.Value;

            // Fallback to legacy
            if (legacyVariable is VariableBase<T> legacyTyped)
                return legacyTyped.Value;

            return default;
        }

        public void Set<T>(T val)
        {
            // Prefer modern variable
            if (variable is IVariable<T> typed)
            {
                typed.Value = val;
                return;
            }

            // Fallback to legacy
            if (legacyVariable is VariableBase<T> legacyTyped)
            {
                legacyTyped.Value = val;
            }
        }
    }
}