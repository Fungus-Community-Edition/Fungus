using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets a boolean parameter on an Animator component to control a Unity animation"
    /// </summary>
    [CommandInfo("Animation", 
                 "Set Anim Bool", 
                 "Sets a boolean parameter on an Animator component to control a Unity animation")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetAnimBool : Command
    {
        [Tooltip("Reference to an Animator component in a game object")]
        [SerializeField] protected AnimatorData _animator;

        [Tooltip("Name of the boolean Animator parameter that will have its value changed")]
        [SerializeField] protected StringData _parameterName;

        [Tooltip("The boolean value to set the parameter to")]
        [SerializeField] protected BooleanData value;

        #region Public members

        public override void OnEnter()
        {
            if (_animator.Value != null)
            {
                _animator.Value.SetBool(_parameterName.Value, value.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_animator.Value == null)
            {
                return "Error: No animator selected";
            }

            return _animator.Value.name + " (" + _parameterName.Value + ")";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Animation;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_animator.VarRef, variable) || 
                ReferenceEquals(_parameterName.VarRef, variable) || 
                ReferenceEquals(value.VarRef, variable) ||
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_animator);
            _variableDataCache.Add(_parameterName);
            _variableDataCache.Add(value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (animatorOLD != null)
            {
                _animator.Value = animatorOLD;
                animatorOLD = null;
            }

            if (parameterNameOLD != "")
            {
                _parameterName.Value = parameterNameOLD;
                parameterNameOLD = "";
            }
        }

        [HideInInspector][FormerlySerializedAs("animator")] public Animator animatorOLD;
        [HideInInspector][FormerlySerializedAs("parameterName")] public string parameterNameOLD = "";

        #endregion
    }
}
