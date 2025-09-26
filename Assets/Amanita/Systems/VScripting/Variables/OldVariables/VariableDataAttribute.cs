using System;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    /// <summary>
    /// For VariableData subclasses
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
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