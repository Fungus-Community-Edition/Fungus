using UnityEngine;
using UnityEngine.Serialization;

namespace Amanita.VScripting
{
    /// <summary>
    /// Sets a game object in the scene to be active / inactive.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Set Active", 
                 "Sets a game object in the scene to be active / inactive.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class SetActive : Command
    {
        [Tooltip("Reference to game object to enable / disable")]
        [SerializeField] protected GameObjectData _targetGameObject;

        [Tooltip("Set to true to enable the game object")]
        [SerializeField] protected BooleanData activeState;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_targetGameObject);
            variableDataCache.Add(activeState);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_targetGameObject.Value != null)
            {
                _targetGameObject.Value.SetActive(activeState.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_targetGameObject.Value == null)
            {
                return "Error: No game object selected";
            }

            return _targetGameObject.Value.name + " = " + activeState.GetDescription();
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_targetGameObject.VarRef, variable) || 
                ReferenceEquals(activeState.VarRef, variable) || 
                base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("targetGameObject")] public GameObject targetGameObjectOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (targetGameObjectOLD != null)
            {
                _targetGameObject.Value = targetGameObjectOLD;
                targetGameObjectOLD = null;
            }
        }

        #endregion
    }
}
