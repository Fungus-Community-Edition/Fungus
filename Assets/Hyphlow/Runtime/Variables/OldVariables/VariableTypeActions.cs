using System;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
[MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public class VariableTypeActions : IEquatable<VariableTypeActions>
    {
        public Func<IVariable, IVariableData, CompareOperator, bool> CompareFunc = (ivar, ivardata, op)
            => false;
        public Func<IVariableData, string> DescFunc = (ivar) => string.Empty;
        public Action<IVariable, IVariableData, SetOperator> SetFunc = delegate { };

        // To help evaluate two different VariableTypeActions
        public virtual string CompareFuncID { get; set; } = string.Empty;
        public virtual string DescFuncID { get; set; } = string.Empty;
        public virtual string SetFuncID { get; set; } = string.Empty;

        public virtual bool Equals(VariableTypeActions other)
        {
            bool result = ReferenceEquals(this.CompareFunc, other.CompareFunc) &&
                ReferenceEquals(this.SetFunc, other.SetFunc) &&
                ReferenceEquals(this.DescFunc, other.DescFunc);
            //bool result = CompareFuncID == other.CompareFuncID &&
            //    DescFuncID == other.DescFuncID &&
            //    SetFuncID == other.SetFuncID;
            return result;
        }
    }
}