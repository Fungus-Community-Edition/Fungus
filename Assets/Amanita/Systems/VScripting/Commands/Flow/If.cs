using UnityEngine;

namespace Amanita.VScripting.Commands
{
    /// <summary>
    /// If the test expression is true, execute the following command block.
    /// </summary>
    [CommandInfo("Flow", 
                 "If", 
                 "If the test expression is true, execute the following command block.")]
    [AddComponentMenu("")]
    public class If : VariableCondition
    {
    }
}