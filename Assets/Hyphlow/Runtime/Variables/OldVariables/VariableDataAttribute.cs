using System;
using System.Collections.Generic;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// For VariableData subclasses
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
[MovedFrom("AtMycelia.Hyphlow")]
    public sealed class VariableDataAttribute : Attribute
    {
        public Type ContentType { get; private set; }
        public IList<Type> VariableTypes { get; private set; }

        public VariableDataAttribute(Type contentType, params Type[] variableType)
        {
            ContentType = contentType;
            VariableTypes = variableType;
        }

    }
}