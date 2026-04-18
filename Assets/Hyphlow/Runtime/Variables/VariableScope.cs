using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Scope types for Variables.
    /// </summary>
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public enum VariableScope
    {
        /// <summary> Can only be accessed by commands in the same Flowchart. </summary>
        Private,
        /// <summary> Can be accessed from any command in any Flowchart. </summary>
        Public,
        Global
    }
}