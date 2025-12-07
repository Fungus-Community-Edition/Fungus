using UnityEngine;
using System.Collections.Generic;

namespace Amanita.VScripting
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