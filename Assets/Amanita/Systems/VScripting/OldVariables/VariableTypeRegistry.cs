using System;
using System.Collections.Generic;
using UnityEngine;

namespace Amanita.VScripting
{
    public static class VariableTypeRegistry
    {
        private static readonly List<Type> _types = new();
        private static readonly Dictionary<Type, VariableTypeActions> _actions = new(new TypeNameComparer());

        public static void Clear()
        {
            _types.Clear();
            _actions.Clear();
        }

        public static void Register<TVar>(VariableTypeActions actions) where TVar: IVariable
        {
            var type = typeof(TVar);
            if (!_types.Contains(type))
                _types.Add(type);

            _actions[type] = actions;
        }

        public static IReadOnlyList<Type> AllTypes => new List<Type>(_types);

        public static void Register(Type varType, VariableTypeActions actions)
        {
            if (!_types.Contains(varType))
                _types.Add(varType);

            _actions[varType] = actions;
        }

        public static bool TryGetTypeActionsFor<T>(out VariableTypeActions result)
        {
            Type type = typeof(T);
            return TryGetTypeActionsFor(type, out result);
        }

        public static bool TryGetTypeActionsFor(System.Type type, out VariableTypeActions result)
        {
            bool gotIt = _actions.TryGetValue(type, out result);

            if (!gotIt)
            {
                string logMessage = $"Could not get type actions for type {type.Name}.";
                Debug.LogError(logMessage);
            }

            return gotIt;
        }


    }
}