using AtMycelia.Hyphlow;

namespace AtMycelia.Amanita
{
    /// <summary>
    /// Interface for indicating that the class holds a reference to an Amanita variable, used primarily in editor.
    /// </summary>
    public interface IVariableReference
    {
        bool HasReference(IVariable variable);
    }
}