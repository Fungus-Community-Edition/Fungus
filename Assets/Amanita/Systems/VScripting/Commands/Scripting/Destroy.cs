using UnityEngine;
using UnityEngine.Serialization;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Destroys a specified game object in the scene.
    /// </summary>
    [CommandInfo("Scripting",
                 "Destroy",
                 "Destroys a specified game object in the scene.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class Destroy : Command, ISerializationCallbackReceiver
    {
        [Tooltip("Reference to game object to destroy")]
        [SerializeField] protected GameObjectData _targetGameObject;

        [Tooltip("Optional delay given to destroy")]
        [SerializeField]
        protected FloatData destroyInXSeconds = new FloatData(0);

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            variableDataCache.Add(_targetGameObject);
            variableDataCache.Add(destroyInXSeconds);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_targetGameObject.Value != null)
            {
                if (destroyInXSeconds.Value != 0)
                    Destroy(_targetGameObject, destroyInXSeconds.Value);
                else
                    Destroy(_targetGameObject.Value);
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_targetGameObject.Value == null)
            {
                return "Error: No game object selected";
            }

            return _targetGameObject.Value.name + (destroyInXSeconds.Value == 0 ? "" : " in " + destroyInXSeconds.Value.ToString());
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(_targetGameObject.VarRef, variable) || ReferenceEquals(destroyInXSeconds.VarRef, variable))
                return true;

            return false;
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("targetGameObject")] public GameObject targetGameObjectOLD;

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
            destroyInXSeconds ??= new FloatData(0);
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
