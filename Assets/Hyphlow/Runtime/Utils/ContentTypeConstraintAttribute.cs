using UnityEngine;
using System.Collections.Generic;
using System;

namespace AtMycelia.Hyphlow
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
            if (types == null)
            {
                AllowedTypes = Array.Empty<Type>();
            }
            else
            {
                AllowedTypes = types;
            }
            
        }

        public IList<System.Type> AllowedTypes { get; }
    }
}