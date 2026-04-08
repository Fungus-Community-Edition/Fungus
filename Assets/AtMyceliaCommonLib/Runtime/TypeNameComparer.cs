using System;
using System.Collections.Generic;

namespace AtMycelia
{
    public class TypeNameComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
          => String.Equals(x?.AssemblyQualifiedName, y?.AssemblyQualifiedName, StringComparison.Ordinal);

        public int GetHashCode(Type t)
          => t.AssemblyQualifiedName.GetHashCode();
    }
}