using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Destroys a specified game object in the scene.
    /// </summary>
    [CommandInfo("Scripting",
                 "Destroy",
                 "Destroys a specified game object in the scene.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Destroy : Command, ISerializationCallbackReceiver
    {
        [Tooltip("Reference to game object to destroy")]
        [SerializeField] protected GameObjectData _targetGameObject;

        [Tooltip("Optional delay given to destroy")]
        [FormerlySerializedAs("destroyInXSeconds")]
        [SerializeField]
        protected FloatData _destroyInXSeconds = new FloatData(0);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_targetGameObject);
            _variableDataCache.Add(_destroyInXSeconds);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_targetGameObject.Value != null)
            {
                if (_destroyInXSeconds.Value != 0)
                {
                    Destroy(_targetGameObject, _destroyInXSeconds.Value);
                }
                else
                {
                    Destroy(_targetGameObject.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            string result;
            if (_targetGameObject.Value == null && !_targetGameObject.RepresentingVar)
            {
                result = "Error: No game object selected";
            }
            else
            {
                result = _targetGameObject.RepresentingVar ? 
                    $"{_targetGameObject.VarRef.Key}" : 
                    $"{_targetGameObject.Value.name}";
                if (_destroyInXSeconds.Value != 0)
                {
                    result += $" in {_destroyInXSeconds.Value} seconds";
                }
            }

            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(_targetGameObject.VarRef, variable) || ReferenceEquals(_destroyInXSeconds.VarRef, variable))
                return true;

            return false;
        }

        protected virtual void OnDestroy()
        {
            CancelInvoke();
            StopAllCoroutines();
        }

        #endregion

        #region Backwards compatibility


        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            _destroyInXSeconds ??= new FloatData(0);
            if (targetGameObjectOLD != null)
            {
                _targetGameObject.Value = targetGameObjectOLD;
                targetGameObjectOLD = null;
            }
        }

        [HideInInspector][FormerlySerializedAs("targetGameObject")] public GameObject targetGameObjectOLD;



        #endregion
    }
}
