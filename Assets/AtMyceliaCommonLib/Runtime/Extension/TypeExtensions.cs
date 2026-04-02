using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Type = System.Type;

namespace AtMycelia
{
    public static class TypeExtensions
    {
        public static bool IsConcrete(this Type type)
        {
            return !(type.IsAbstract || type.IsInterface);
        }

        public static IList<Type> GetInstantiatableTypes(this Type baseType)
        {
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            IList<Type> result = allAssemblies.SelectMany(SafeGetTypes)
                         .Where((elem) => IsInstantiatableType(elem, baseType))
                         .ToList();

            return result;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly toGetTypesFrom)
        {
            try
            {
                return toGetTypesFrom.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(typeFound => typeFound != null);
            }
        }

        public static bool IsInstantiatableType(Type typeToCheck, Type baseType)
        {
            return baseType.IsAssignableFrom(typeToCheck)
                   && !typeToCheck.IsAbstract
                   && !typeToCheck.IsInterface;
        }
    }
}