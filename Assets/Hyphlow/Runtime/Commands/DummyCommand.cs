using UnityEngine;

namespace AtMycelia.Hyphlow
{
    [CommandInfo("Debug", 
                 "Dummy Command", 
                 "This is a dummy command used for testing and debugging purposes.")]
    public class DummyCommand : Command
    {
        [ContentTypeConstraint(typeof(int), typeof(GameObject), typeof(string))]
        [SerializeField] protected AnyVariableData _anyVariableData = new AnyVariableData();
        [SerializeField] protected AnyVariableAndDataPair _anyVariableAndDataPair = new AnyVariableAndDataPair();
        [SerializeField] protected ComponentData _componentData = new ComponentData();
    }
}