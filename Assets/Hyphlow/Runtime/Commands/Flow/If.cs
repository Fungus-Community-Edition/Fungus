using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// If the test expression is true, execute the following command block.
    /// </summary>
    [CommandInfo("Flow", 
                 "If", 
                 "If the test expression is true, execute the following command block.")]
    [AddComponentMenu("")]
    [MovedFrom("AtMycelia.Amanita.VScripting.Commands")]
    public class If : VariableCondition
    {
    }
}