using System;

namespace Amanita
{
    public static class TypeExtensions
    {
        public static bool IsConcrete(this Type type)
        {
            return !(type.IsAbstract || type.IsInterface);
        }
    }
}