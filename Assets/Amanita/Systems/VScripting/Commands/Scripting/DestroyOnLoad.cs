using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// Calls DontDestroyOnLoad on the target gameobject.
    /// </summary>
    [CommandInfo("Scripting",
                 "DestroyOnLoad",
                 "Calls DontDestroyOnLoad on the target gameobject")]
    [AddComponentMenu("")]
    public class DestroyOnLoad : Command
    {
        [SerializeField] protected GameObjectData target;

        public override void OnEnter()
        {
            DontDestroyOnLoad(target.Value);

            Continue();
        }

        public override string GetSummary()
        {
            return target.Value != null ? target.Value.name : "Error: no target set";
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(variable, target.VarRef);
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }
    }
}
