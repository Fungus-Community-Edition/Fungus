using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Calls DontDestroyOnLoad on the target gameobject.
    /// </summary>
    [CommandInfo("Scripting",
                 "DestroyOnLoad",
                 "Calls DontDestroyOnLoad on the target gameobject")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class DestroyOnLoad : Command
    {
        [SerializeField] protected GameObjectData target;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(target);
        }

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
