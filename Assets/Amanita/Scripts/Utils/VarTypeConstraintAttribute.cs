using UnityEngine;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    public class VarTypeConstraintAttribute : PropertyAttribute
    {
        public VarTypeConstraintAttribute(params System.Type[] types)
        {
            AllowedTypes = types;
        }

        public IList<System.Type> AllowedTypes { get; }
    }
}