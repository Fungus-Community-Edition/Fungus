using System;
using System.Collections.Generic;
using System.Reflection;
using Type = System.Type;
using System.Linq;

namespace AtMycelia
{
    public static class TypeUtils
    {
        public static bool TypesCompatible(Type firstType, Type secondType)
        {
            bool regularAssignability = firstType.IsAssignableFrom(secondType);
            bool castableNumericTypes = _basicNumericTypes.Contains(firstType) && 
                _basicNumericTypes.Contains(secondType);
            bool result = regularAssignability || castableNumericTypes;
            return result;
        }

        private static readonly Type[] _basicNumericTypes = new Type[]
        {
            typeof(sbyte), typeof(byte),
            typeof(short), typeof(ushort),
            typeof(int), typeof(uint),
            typeof(long), typeof(ulong),
            typeof(char), typeof(float),
            typeof(double), typeof(decimal)
        };

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