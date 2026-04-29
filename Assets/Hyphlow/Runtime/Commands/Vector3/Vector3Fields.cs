using UnityEngine;

using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Get or Set the x,y,z fields of a vector3 via floatvars
    /// </summary>
    [CommandInfo("Vector3",
                 "Fields",
                 "Get or Set the x,y,z fields of a vector3 via floatvars")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Vector3Fields : Command
    {
        public enum GetSet
        {
            Get,
            Set,
        }

        [SerializeField]
        [FormerlySerializedAs("getOrSet")]
        private GetSet _getOrSet = GetSet.Get;

        public GetSet GetOrSet
        {
            get => _getOrSet;
            set => _getOrSet = value;
        }

        [SerializeField]
        [ContentTypeConstraint(typeof(Vector3), typeof(Vector2))]
        protected VariableReference _vec3Var;

        [SerializeField]
        [FormerlySerializedAs("x")]
        protected FloatData _x;

        [SerializeField]
        [FormerlySerializedAs("y")]
        protected FloatData _y;

        [SerializeField]
        [FormerlySerializedAs("z")]
        protected FloatData _z;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(_x);
            _variableDataCache.Add(_y);
            _variableDataCache.Add(_z);
        }

        public override void OnEnter()
        {
            switch (_getOrSet)
            {
                case GetSet.Get:

                    var v = _vec3Var.GetValue<Vector3>();

                    _x.Value = v.x;
                    _y.Value = v.y;
                    _z.Value = v.z;
                    break;
                case GetSet.Set:
                    Vector3 newVal = new Vector3(_x.Value, _y.Value, _z.Value);
                    Debug.Log($"Setting vector3 to {newVal}");
                    _vec3Var.SetValue(newVal);
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (_vec3Var.Variable == null)
            {
                return "Error: vec3 not set";
            }

            return _getOrSet.ToString() + " (" + _vec3Var.Variable.Key + ")";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(_vec3Var.Variable, variable) || 
                ReferenceEquals(_x.VarRef, variable) || 
                ReferenceEquals(_y.VarRef, variable) || 
                ReferenceEquals(_z.VarRef, variable))
                return true;

            return false;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (vec3 != null)
            {
                if (vec3.RepresentingVar)
                {
                    _vec3Var.Variable = vec3.VarRef;
                }

                vec3 = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("vec3")]
        [HideInInspector]
        protected Vector3Data vec3;
    }
}
