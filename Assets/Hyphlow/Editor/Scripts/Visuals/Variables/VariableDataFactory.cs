using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtMycelia.Hyphlow
{
    public static class VariableDataFactory
    {
        public static IVariableData CreateForVar<TVarType>() where TVarType : IVariable
        {
            return CreateForVar(typeof(TVarType));
        }

        public static IVariableData CreateForVar(Type variableType)
        {
            IVariableData result = null;
            string logMessage = "";
            if (variableType == null)
            {
                logMessage = "Cannot create variable data for a null var type. Returning null.";
                Debug.LogWarning(logMessage);
            }
            else
            {
                TypeMap.TryGetValue(variableType, out var dataType);
                if (dataType != null)
                {
                    result = (IVariableData)Activator.CreateInstance(dataType);
                }
                else
                {
                    int typeCount = TypeMap.Count;
                    logMessage = $"Couldn't make an instance for {variableType.Name}. The amount of types " +
                        $"in the registry: {typeCount}. Returning null.";
                    Debug.LogWarning(logMessage);
                }
            }

            return result;
        }

        private static 
            IReadOnlyDictionary<Type, Type> TypeMap => VariableDataTypeRegistry.TypeMap;
    }
}