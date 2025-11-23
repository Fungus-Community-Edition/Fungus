using System;
using System.Collections.Generic;
using System.Reflection;
using Type = System.Type;
using System.Linq;

namespace Amanita
{
    public static class TypeUtils
    {
        public static IList<Type> GetInstantiatableTypes(Type baseType)
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