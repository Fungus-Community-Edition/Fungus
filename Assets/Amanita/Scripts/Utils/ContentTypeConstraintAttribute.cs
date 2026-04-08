using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Attribute for constraining the allowed types of a VariableReference property to a 
    /// specified list of types. Used to ensure that only compatible variable types can be 
    /// assigned to a VariableReference field in the inspector.
    /// </summary>
    public class ContentTypeConstraintAttribute : PropertyAttribute
    {
        public ContentTypeConstraintAttribute(params System.Type[] types)
        {
            AllowedTypes = types;
        }

        public IList<System.Type> AllowedTypes { get; }
    }
}