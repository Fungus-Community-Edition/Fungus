using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Amanita.VScripting
{
    public class ContentTypeConstraintAttribute : PropertyAttribute
    {
        public ContentTypeConstraintAttribute(params System.Type[] types)
        {
            AllowedTypes = types;
        }

        public IList<System.Type> AllowedTypes { get; }
    }
}