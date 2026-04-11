using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Sets a game object in the scene to be active / inactive.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Set Active", 
                 "Sets a game object in the scene to be active / inactive.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SetActive : Command
    {
        [Tooltip("Reference to game object to enable / disable")]
        [SerializeField] protected GameObjectData _targetGameObject = new GameObjectData();

        [Tooltip("Set to true to enable the game object")]
        [FormerlySerializedAs("activeState")]
        [SerializeField] protected BooleanData _activeState = new BooleanData();

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetGameObject);
            _variableDataCache.Add(_activeState);
        }

        public override void OnEnter()
        {
            if (_targetGameObject.Value != null)
            {
                _targetGameObject.Value.SetActive(_activeState.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_targetGameObject.Value == null)
            {
                return "Error: No game object selected";
            }

            string result = "";

            if (_targetGameObject.RepresentingVar)
            {
                result += $"{_targetGameObject.VarRef.Key} ";
            }
            else
            {
                GameObject targGo = _targetGameObject.Value;
                result += $"{targGo.name} ";
            }

            result += $"= {_activeState.GetDescription()}";
            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            return ReferenceEquals(_targetGameObject.VarRef, variable) || 
                ReferenceEquals(_activeState.VarRef, variable) || 
                base.HasReference(variable);
        }


        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("targetGameObject")] public GameObject targetGameObjectOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (targetGameObjectOLD != null)
            {
                _targetGameObject.Value = targetGameObjectOLD;
                targetGameObjectOLD = null;
            }
        }

        #endregion
    }
}
