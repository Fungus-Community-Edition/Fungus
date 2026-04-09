namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Interface to allow the VariableEditor to check that a given fungus variable is compatibile with the object it resides
    /// within, such as in the collection base command
    /// </summary>
    public interface ICollectionCompatible
    {
        bool IsVarCompatibleWithCollection(Variable variableInQuestion, string compatibleWith);
    }
}