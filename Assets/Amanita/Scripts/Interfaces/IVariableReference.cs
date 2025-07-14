// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using Amanita.VScripting;

namespace Amanita
{
    /// <summary>
    /// Interface for indicating that the class holds a reference to a fungus variable, used primarily in editor.
    /// </summary>
    public interface IVariableReference : IStringLocationIdentifier
    {
        bool HasReference(IVariable variable);
    }
}