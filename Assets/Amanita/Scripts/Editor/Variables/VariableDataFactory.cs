using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableDataFactory
    {
        public static IVariableData CreateForVar<TVarType>() where TVarType : IVariable
        {
            return CreateForVar(typeof(TVarType));
        }

        public static IVariableData CreateForVar(Type variableType)
        {
            var success = TypeMap.TryGetValue(variableType, out var dataType);
            IVariableData result = null;
            if (dataType != null)
            {
                result = (IVariableData)Activator.CreateInstance(dataType);
            }
            else
            {
                int typeCount = TypeMap.Count;
                Debug.Log($"Couldn't make an instance for {variableType.Name}. The amount of types " +
                    $"in the registry: {typeCount}");
            }

            return result;
        }

        private static 
            IReadOnlyDictionary<Type, Type> TypeMap => VariableDataTypeRegistry.TypeMap;
    }
}