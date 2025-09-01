using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableDataRegistry
    {
        // Key is var type, value is data type
        private static readonly Dictionary<Type, Type> _varTypeToDataType = new();

        public static void Clear()
        {
            _varTypeToDataType.Clear();
        }

        public static void Register(Type varDataType, VariableDataAttribute attr)
        {
            if (attr == null || varDataType == null)
            {
                Debug.LogWarning($"Passed null attr or varDataType to VariableDataRegistry Register func");
                return;
            }

            IList<Type> compatibleVarTypes = attr.VariableTypes.Where((elem) => elem != null).ToList();

            foreach (var varTypeEl in compatibleVarTypes)
            {
                _varTypeToDataType.TryAdd(varTypeEl, varDataType);
            }
        }

        /// <summary>
        /// T is the variable type (IntegerVariable, AudioClipVariable, etc)
        /// </summary>
        public static IVariableData CreateForVar<T>() where T: IVariable
        {
            return CreateForVar(typeof(T));
        }

        public static IVariableData CreateForVar(Type variableType)
        {
            var dataType = GetDataTypeLinkedToVarType(variableType);
            IVariableData result = null;
            if (dataType != null)
            {
                result = (IVariableData)Activator.CreateInstance(dataType);
            }
            else
            {
                Debug.Log($"Couldn't make an instance for {variableType.Name}. The amount of types " +
                    $"in the registry: {_varTypeToDataType.Count}");
            }
            
            return result;
        }

        public static Type GetDataTypeLinkedToVarType(Type variableType)
        {
            _varTypeToDataType.TryGetValue(variableType, out var result);
            return result;
        }

    }

}