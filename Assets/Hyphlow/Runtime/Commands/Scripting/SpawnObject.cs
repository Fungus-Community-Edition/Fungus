using UnityEngine;
using UnityEngine.Serialization;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Spawns a new object based on a reference to a scene or prefab game object.
    /// </summary>
    [CommandInfo("Scripting", 
                 "Spawn Object", 
                 "Spawns a new object based on a reference to a scene or prefab game object.", 
        Priority = 10)]
    [CommandInfo("GameObject",
                 "Instantiate",
                 "Instantiate a game object")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class SpawnObject : Command
    {
        [Tooltip("Game object to copy when spawning. Can be a scene object or a prefab.")]
        [SerializeField] protected GameObjectData _sourceObject;

        [Tooltip("Transform to use as parent during instantiate.")]
        [SerializeField] protected TransformData _parentTransform;

        [Tooltip("If true, will use the Transfrom of this Flowchart for the position and rotation.")]
        [SerializeField] protected BooleanData _spawnAtSelf = new BooleanData(false);

        [Tooltip("Local position of newly spawned object.")]
        [SerializeField] protected Vector3Data _spawnPosition;

        [Tooltip("Local rotation of newly spawned object.")]
        [SerializeField] protected Vector3Data _spawnRotation;

        [Tooltip("Optional variable to store the GameObject that was just created.")]
        [SerializeField]
        [ContentTypeConstraint(typeof(GameObject))]
        protected VariableReference _newlySpawnedObject;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_sourceObject);
            _variableDataCache.Add(_parentTransform);
            _variableDataCache.Add(_spawnAtSelf);
            _variableDataCache.Add(_spawnPosition);
            _variableDataCache.Add(_spawnRotation);
        }

        #region Public members

        public override void OnEnter()
        {
            if (_sourceObject.Value == null)
            {
                Continue();
                return;
            }

            GameObject newObject = null;

            if (_parentTransform.Value != null)
            {
                newObject = GameObject.Instantiate(_sourceObject.Value, _parentTransform.Value);
            }
            else
            {
                newObject = GameObject.Instantiate(_sourceObject.Value);
            }

            if (!_spawnAtSelf.Value)
            {
                Quaternion spawnRot = Quaternion.Euler(_spawnRotation.Value);
                newObject.transform.SetLocalPositionAndRotation(_spawnPosition.Value, spawnRot);
            }
            else
            {
                newObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            }

            if (_newlySpawnedObject.Variable != null)
            {
                _newlySpawnedObject.SetValue(newObject);
            }

            Continue();
        }

        public override string GetSummary()
        {
            string result = "Error: No source GameObject specified";
            if (_sourceObject.Value != null)
            {
                if (_sourceObject.RepresentingVar)
                {
                    result = $"Spawn from {_sourceObject.VarRef.Key}";
                }
                else
                {
                    result = $"Spawn {_sourceObject.Value.name}";
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
            if (ReferenceEquals(_sourceObject.VarRef, variable) || 
                ReferenceEquals(_parentTransform.VarRef, variable) ||
                ReferenceEquals(_spawnAtSelf.VarRef, variable) || 
                ReferenceEquals(_spawnPosition.VarRef, variable) ||
                ReferenceEquals(_spawnRotation.VarRef, variable))
                return true;

            return false;
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("sourceObject")] public GameObject sourceObjectOLD;
        [HideInInspector] [FormerlySerializedAs("parentTransform")] public Transform parentTransformOLD;
        [HideInInspector] [FormerlySerializedAs("spawnPosition")] public Vector3 spawnPositionOLD;
        [HideInInspector] [FormerlySerializedAs("spawnRotation")] public Vector3 spawnRotationOLD;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (sourceObjectOLD != null)
            {
                _sourceObject.Value = sourceObjectOLD;
                sourceObjectOLD = null;
            }
            if (parentTransformOLD != null)
            {
                _parentTransform.Value = parentTransformOLD;
                parentTransformOLD = null;
            }
            if (spawnPositionOLD != default(Vector3))
            {
                _spawnPosition.Value = spawnPositionOLD;
                spawnPositionOLD = default(Vector3);
            }
            if (spawnRotationOLD != default(Vector3))
            {
                _spawnRotation.Value = spawnRotationOLD;
                spawnRotationOLD = default(Vector3);
            }
        }

        #endregion
    }
}
