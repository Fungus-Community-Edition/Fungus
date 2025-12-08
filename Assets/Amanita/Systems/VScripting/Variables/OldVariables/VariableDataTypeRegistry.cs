using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableDataTypeRegistry
    {
        // Key is var type, value is data type
        private static readonly IDictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        /// <summary>
        /// Key: IVariable-implementor.
        /// Value: IVariableData-implementor.
        /// </summary>
        public static IReadOnlyDictionary<Type, Type> TypeMap => 
            _typeMap as IReadOnlyDictionary<Type, Type>;

        public static void Clear()
        {
            _typeMap.Clear();
        }

        public static void Register(Type varDataType)
        {
            if (varDataType == null)
            {
                Debug.LogWarning($"Passed null varDataType to VariableDataRegistry Register func");
                return;
            }

            VariableDataAttribute attr = varDataType.GetCustomAttribute<VariableDataAttribute>();

            IList<Type> compatibleVarTypes = attr.VariableTypes.Where((elem) => elem != null).ToList();

            foreach (var varTypeEl in compatibleVarTypes)
            {
                _typeMap.TryAdd(varTypeEl, varDataType);
            }
        }

        public static IVariableData CreateForVar<TVarType>() where TVarType: IVariable
        {
            return CreateForVar(typeof(TVarType));
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
                    $"in the registry: {_typeMap.Count}");
            }
            
            return result;
        }

        public static Type GetDataTypeLinkedToVarType(Type variableType)
        {
            // For polymorphism, we need to check assignability
            foreach (var val in _typeMap.Values)
            {
                VariableDataAttribute attr = val.GetCustomAttribute<VariableDataAttribute>();
                if (attr != null)
                {
                    foreach (var varTypeEl in attr.VariableTypes)
                    {
                        if (varTypeEl != null && varTypeEl.IsAssignableFrom(variableType))
                        {
                            return val;
                        }
                    }
                }
            }
            return null;
        }

    }

}