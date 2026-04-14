using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Plays a state of an animator according to the state name.
    /// </summary>
    [CommandInfo("Animation", 
                 "Play Anim State", 
                 "Plays a state of an animator according to the state name")]
    [AddComponentMenu("")]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class PlayAnimState : Command 
    {
        [Tooltip("Reference to an Animator component in a game object")]
        [SerializeField] protected AnimatorData animator = new AnimatorData();

        [Tooltip("Name of the state you want to play")]
        [SerializeField] protected StringData stateName = new StringData();

        [Tooltip("Layer to play animation on")]
        [SerializeField] protected IntegerData layer = new IntegerData(-1);

        [Tooltip("Start time of animation")]
        [SerializeField] protected FloatData time = new FloatData(0f);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(animator);
            _variableDataCache.Add(stateName);
            _variableDataCache.Add(layer);
            _variableDataCache.Add(time);
        }

        #region Public members

        public override void OnEnter()
        {
            if (animator.Value != null)
            {
                animator.Value.Play(stateName.Value, layer.Value, time.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (animator.Value == null)
            {
                return "Error: No animator selected";
            }

            return animator.Value.name + " (" + stateName.Value + ")";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(animator.VarRef, variable) || ReferenceEquals(stateName.VarRef, variable) || 
                ReferenceEquals(layer.VarRef, variable) || ReferenceEquals(time.VarRef, variable) || 
                base.HasReference(variable);
        }

        #endregion
    }    
}

