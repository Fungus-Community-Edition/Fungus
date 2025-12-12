using Amanita.VScripting;

namespace Amanita
{
    /// <summary>
    /// Interface for indicating that the class holds a reference to an Amanita variable, used primarily in editor.
    /// </summary>
    public interface IVariableReference : IStringLocationIdentifier
    {
        bool HasReference(IVariable variable);
    }
}