using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Normalise a vector3, output can be the same as the input
    /// </summary>
    [CommandInfo("Vector3",
                 "Normalise",
                 "Normalise a Vector3")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Vector3Normalise : Command
    {
        [SerializeField]
        [FormerlySerializedAs("vec3In")]
        protected Vector3Data vecThreeIn;

        [SerializeField]
        [ContentTypeConstraint(typeof(Vector3))]
        protected VariableReference _vecThreeOut;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(vecThreeIn);
        }

        public override void OnEnter()
        {
            var normalized = vecThreeIn.Value.normalized;
            _vecThreeOut.SetValue(normalized);
            Continue();
        }

        public override string GetSummary()
        {
            if (_vecThreeOut.Variable == null)
                return "Needs output var";
            else
                return _vecThreeOut.Variable.Key;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(vecThreeIn.VarRef, variable) || 
                ReferenceEquals(_oldVecThreeOut.VarRef, variable))
                return true;

            return false;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();
            if (_oldVecThreeOut != null)
            {
                if (_oldVecThreeOut.VarRef != null)
                {
                    _vecThreeOut.Variable = _oldVecThreeOut.VarRef;
                }

                _oldVecThreeOut = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("_vec3Out")]
        [FormerlySerializedAs("vec3Out")]
        [HideInInspector]
        protected Vector3Data _oldVecThreeOut;
    }
}
