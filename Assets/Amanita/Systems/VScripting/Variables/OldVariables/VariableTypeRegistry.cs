using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableTypeRegistry
    {
        private static readonly HashSet<Type> _legacyTypes = new();
        private static readonly HashSet<Type> _muscariableTypes = new();
        private static readonly Dictionary<Type, VariableTypeActions> _actions = new(new TypeNameComparer());

        public static IReadOnlyList<Type> AllLegacyTypes => _legacyTypes.ToArray();
        public static IReadOnlyList<Type> AllMuscariableTypes => _muscariableTypes.ToArray();

        public static void RegisterVariable(Type varType, VariableTypeActions actions, bool isLegacy)
        {
            if (isLegacy)
            {
                _legacyTypes.Add(varType);
            }
            else
            {
                _muscariableTypes.Add(varType);
            }

            _actions[varType] = actions;
        }

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

        private static IDictionary<Type, Type> _contentTypeToVarType = new Dictionary<Type, Type>(new TypeNameComparer());

        public static Type MuscariableTypeFor(Type contentType)
        {
            return VarTypeFor(_muscariableTypes, contentType);
        }

        public static bool TryGetTypeActionsFor<T>(out VariableTypeActions result)
        {
            Type type = typeof(T);
            return TryGetTypeActionsFor(type, out result);
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

        private static Variable CreateLegacyVar(Type legacyVarType, Flowchart varHolder)
        {
            Variable result = null;
            var newVariable = varHolder.gameObject.AddComponent(legacyVarType) as Variable;
            if (newVariable == null)
            {
                Debug.LogError($"Failed to add variable component of type {legacyVarType.Name} to {varHolder.name}");
                
            }
            else
            {
                result = newVariable;
            }

            return result;
        }

        private static Muscariable CreateMuscariable(Type muscariType)
        {
            Muscariable result = (Muscariable)Activator.CreateInstance(muscariType);
            return result;
        }

        public static void Clear()
        {
            _contentTypeToVarType.Clear();
            _legacyTypes.Clear();
            _muscariableTypes.Clear();
            _actions.Clear();
        }

    }
}