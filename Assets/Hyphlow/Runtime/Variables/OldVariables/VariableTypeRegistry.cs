using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnityEngine.Scripting.APIUpdating;

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// A registry and instantiator for legacy and muscari types alike.
    /// </summary>
    [MovedFrom(true, "AtMycelia.Hyphlow", "AtMycelia.Amanita.Core")]
    public static class VariableTypeRegistry
    {
        private static readonly IList<Type> _legacyTypes = new List<Type>();
        private static readonly IList<Type> _muscariableTypes = new List<Type>();
        private static readonly IDictionary<Type, VariableTypeActions> _actionsRegistered = 
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
            bool alreadyRegistered = _legacyTypes.Contains(varType) || _muscariableTypes.Contains(varType);
            bool sameActions = false;
            string logMessage = "";
            if (alreadyRegistered)
            {
                sameActions = actions.Equals(_actionsRegistered[varType]);
            }

            if (alreadyRegistered && sameActions)
            {
                return;
            }

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
                _typeMap[varType] = att.ContentType;
                _actionsRegistered[varType] = actions;

                if (sameActions)
                {
                    logMessage = $"Overwrote actions tied to {varType.Name}.";
                    Debug.Log(logMessage);
                }
            }
        }

        private static readonly Type _baseLegacyType = typeof(Variable);

        public static Type LegacyTypeFor(Type contentType)
        {
            return VarTypeFor(_legacyTypes, _contentTypeToLegacyVarType, contentType);
        }

        private static Type VarTypeFor(IEnumerable<Type> varTypesToCheck, IDictionary<Type, Type> typeLookup, Type contentType)
        {
            Type result = null;

            bool alreadyRegistered = typeLookup.TryGetValue(contentType, out result);
            if (!alreadyRegistered)
            {
                foreach (var varType in varTypesToCheck)
                {
                    VariableInfoAttribute attr = varType.GetCustomAttribute<VariableInfoAttribute>();
                    if (attr != null && attr.ContentType.Equals(contentType))
                    {
                        result = varType;
                        typeLookup.Add(attr.ContentType, varType);
                        break;
                    }
                }
            }
            
            return result;
        }

        private static readonly IDictionary<Type, Type> _contentTypeToLegacyVarType = new Dictionary<Type, Type>(new TypeNameComparer());
        private static readonly IDictionary<Type, Type> _contentTypeToMuscariType = new Dictionary<Type, Type>(new TypeNameComparer());


        /// <summary>
        /// If there is no Muscariable specifically for the passed content type,
        /// this will return the generic muscariable type.
        /// </summary>
        public static Type MuscariTypeFor(Type contentType)
        {
            Type result = VarTypeFor(_muscariableTypes, _contentTypeToMuscariType, contentType);

            // Generic fallback
            result ??= typeof(GenericMuscariable);
            return result;
        }

        public static bool TryGetTypeActionsFor(Type type, out VariableTypeActions result)
        {
            bool gotIt = _actionsRegistered.TryGetValue(type, out result);

            if (!gotIt)
            {
                string logMessage = $"Could not get type actions for type {type.Name}.";
                Debug.LogError(logMessage);
            }

            return gotIt;
        }

        public static Type MuscariTypeByName(string typeName)
        {
            foreach (var muscariType in _muscariableTypes)
            {
                if (muscariType.Name == typeName)
                {
                    return muscariType;
                }
            }
            return null;
        }

        public static void Clear()
        {
            _typeMap.Clear();
            _contentTypeToLegacyVarType.Clear();
            _legacyTypes.Clear();
            _muscariableTypes.Clear();
            _actionsRegistered.Clear();
        }

    }
}