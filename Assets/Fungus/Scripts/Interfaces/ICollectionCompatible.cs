// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

namespace Amanita
{
    /// <summary>
    /// Interface to allow the VariableEditor to check that a given fungus variable is compatibile with the object it resides
    /// within, such as in the collection base command
    /// </summary>
    public interface ICollectionCompatible
    {
        bool IsVarCompatibleWithCollection(Amanita.Variable variableInQuestion, string compatibleWith);
    }
}