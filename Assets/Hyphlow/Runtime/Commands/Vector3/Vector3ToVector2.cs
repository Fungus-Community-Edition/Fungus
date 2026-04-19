using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Convert fungus vec3 to vec2
    /// </summary>
    [CommandInfo("Vector3",
                 "ToVector2",
                 "Convert Hyphlow Vector3 to Hyphlow Vector2")]
    [AddComponentMenu("")]
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class Vector3ToVector2 : Command
    {
        [SerializeField]
        [ContentTypeConstraint(typeof(Vector3))]
        protected VariableReference _vecThree = new VariableReference();

        [SerializeField]
        [ContentTypeConstraint(typeof(Vector2))]
        protected VariableReference _vecTwo = new VariableReference();

        public override void OnEnter()
        {
            var valToSet = _vecThree.GetValue<Vector3>();
            _vecTwo.SetValue(valToSet);
            Continue();
        }

        public override string GetSummary()
        {
            string result;
            if (_vecThree.Variable != null && _vecTwo.Variable != null)
            {
                result = "Converting " + _vecThree.VarKey + " to " + _vecTwo.VarKey;
            }
            else
            {
                result = "Error: variables not set";
            }

            return result;
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(variable, _vecThree.Variable) || 
                ReferenceEquals(variable, _vecTwo.Variable))
                return true;

            return false;
        }

        public override void ApplyBackwardsCompatibility()
        {
            base.ApplyBackwardsCompatibility();

            if (_oldVecThree != null)
            {
                if (_vecThree.Variable == null && _oldVecThree.VarRef != null)
                {
                    _vecThree.Variable = _oldVecThree.VarRef;
                }

                _oldVecThree = null;
            }

            if (_oldVecTwo != null)
            {
                if (_vecTwo.Variable == null && _oldVecTwo.VarRef != null)
                {
                    _vecTwo.Variable = _oldVecTwo.VarRef;
                }

                _oldVecTwo = null;
            }
        }

        [SerializeField]
        [FormerlySerializedAs("vec3")]
        [FormerlySerializedAs("_vec3")]
        [HideInInspector]
        protected Vector3Data _oldVecThree;

        [SerializeField]
        [FormerlySerializedAs("vec2")]
        [FormerlySerializedAs("_vec2")]
        [HideInInspector]
        protected Vector2Data _oldVecTwo;
    }
}
