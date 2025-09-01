using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Amanita.VScripting
{
    /// <summary>
    /// A registry and instantiator for legacy and muscari types alike.
    /// </summary>
    public static class VariableTypeRegistry
    {
        private static readonly IList<Type> _legacyTypes = new List<Type>();
        private static readonly IList<Type> _muscariableTypes = new List<Type>();
        private static readonly IDictionary<Type, VariableTypeActions> _actions = 
            new Dictionary<Type, VariableTypeActions>(new TypeNameComparer());

        // Key: IVariable-implementor. Value: content.
        private static readonly IDictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();

        public static IReadOnlyList<Type> AllLegacyTypes => _legacyTypes as IReadOnlyList<Type>;
        public static IReadOnlyList<Type> AllMuscariableTypes => _muscariableTypes as IReadOnlyList<Type>;
        public static IReadOnlyDictionary<Type, Type> TypeMap => _typeMap as IReadOnlyDictionary<Type, Type>;

        public static void RegisterMultiVariableTypes(IEnumerable<Type> types, VariableTypeActions actions)
        {
            foreach (var type in types)
            {
                RegisterVariableType(type, actions);
            }
        }

        public static void RegisterVariableType(Type varType, VariableTypeActions actions)
        {
            bool isLegacy = _baseLegacyType.IsAssignableFrom(varType);
            if (isLegacy)
            {
                _legacyTypes.Add(varType);
            }
            else
            {
                _muscariableTypes.Add(varType);
            }

            VariableInfoAttribute att = varType.GetCustomAttribute<VariableInfoAttribute>();
            if (att != null)
            {
                _typeMap.Add(varType, att.ContentType);
                _actions[varType] = actions;
            }
        }

        private static readonly Type _baseLegacyType = typeof(Variable);

        public static Type LegacyTypeFor(Type contentType)
        {
            return VarTypeFor(_legacyTypes, contentType);
        }

        private static Type VarTypeFor(IEnumerable<Type> varTypesToCheck, Type contentType)
        {
            Type result = null;

            bool alreadyRegistered = _contentTypeToVarType.TryGetValue(contentType, out result);
            if (!alreadyRegistered)
            {
                foreach (var varType in varTypesToCheck)
                {
                    VariableInfoAttribute attr = varType.GetCustomAttribute<VariableInfoAttribute>();
                    if (attr.ContentType.Equals(contentType))
                    {
                        result = varType;
                        _contentTypeToVarType.Add(attr.ContentType, varType);
                        break;
                    }
                }
            }

            return result;
        }

        private static readonly IDictionary<Type, Type> _contentTypeToVarType = new Dictionary<Type, Type>(new TypeNameComparer());

        /// <summary>
        /// If there is no Muscariable specifically for the passed content type,
        /// this will return the generic muscariable type.
        /// </summary>
        public static Type MuscariTypeFor(Type contentType)
        {
            Type result = VarTypeFor(_muscariableTypes, contentType);

            // Generic fallback
            if (result == null)
            {
                result = typeof(GenericMuscariable);
            }

            return result;
        }

        public static bool TryGetTypeActionsFor(Type type, out VariableTypeActions result)
        {
            bool gotIt = _actions.TryGetValue(type, out result);

            if (!gotIt)
            {
                string logMessage = $"Could not get type actions for type {type.Name}.";
                Debug.LogError(logMessage);
            }

            return gotIt;
        }

        public static void Clear()
        {
            _typeMap.Clear();
            _contentTypeToVarType.Clear();
            _legacyTypes.Clear();
            _muscariableTypes.Clear();
            _actions.Clear();
        }

    }
}