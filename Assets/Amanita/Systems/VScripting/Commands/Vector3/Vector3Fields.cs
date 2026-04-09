using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Get or Set the x,y,z fields of a vector3 via floatvars
    /// </summary>
    [CommandInfo("Vector3",
                 "Fields",
                 "Get or Set the x,y,z fields of a vector3 via floatvars")]
    [AddComponentMenu("")]
[MovedFrom("AtMycelia.Amanita.VScripting")]
    public class Vector3Fields : Command
    {
        public enum GetSet
        {
            Get,
            Set,
        }
        public GetSet getOrSet = GetSet.Get;

        [SerializeField]
        protected Vector3Data vec3;

        [SerializeField]
        protected FloatData x, y, z;

        protected override void RefreshVariableDataCache()
        {
            base.RefreshVariableDataCache();
            _variableDataCache.Add(vec3);
            _variableDataCache.Add(x);
            _variableDataCache.Add(y);
            _variableDataCache.Add(z);
        }

        public override void OnEnter()
        {
            switch (getOrSet)
            {
                case GetSet.Get:

                    var v = vec3.Value;

                    x.Value = v.x;
                    y.Value = v.y;
                    z.Value = v.z;
                    break;
                case GetSet.Set:
                    vec3.Value = new Vector3(x.Value, y.Value, z.Value);
                    break;
                default:
                    break;
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (vec3.vector3Ref == null)
            {
                return "Error: vec3 not set";
            }

            return getOrSet.ToString() + " (" + vec3.vector3Ref.Key + ")";
        }

        public override Color GetButtonColor()
        {
            return CommandColors.Flow;
        }

        public override bool HasReference(Variable variable)
        {
            if (ReferenceEquals(vec3.VarRef, variable) || 
                ReferenceEquals(x.VarRef, variable) || 
                ReferenceEquals(y.VarRef, variable) || 
                ReferenceEquals(z.VarRef, variable))
                return true;

            return false;
        }
    }
}
