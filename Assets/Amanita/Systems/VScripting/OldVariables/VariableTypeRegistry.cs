using System;
using System.Collections.Generic;

namespace Amanita.VScripting
{
    public static class VariableTypeRegistry
    {
        private static readonly List<Type> _types = new();
        private static readonly Dictionary<Type, AnyVariableAndDataPair.TypeActions> _actions = new();

        public static void Register<TVar>(
            string dataPropName,
            Func<AnyVariableAndDataPair, CompareOperator, bool> compare,
            Func<AnyVariableAndDataPair, string> desc,
            Action<AnyVariableAndDataPair, SetOperator> set) where TVar: IVariable
        {
            var type = typeof(TVar);
            if (!_types.Contains(type))
                _types.Add(type);

            _actions[type] = new AnyVariableAndDataPair.TypeActions(dataPropName, compare, desc, set);
        }

        public static IReadOnlyList<Type> AllTypes => _types;

        public static bool TryGetActions(Type t, out AnyVariableAndDataPair.TypeActions actions)
            => _actions.TryGetValue(t, out actions);

        public static void Register(
        Type variableType,
        string dataPropName,
        Func<AnyVariableAndDataPair, CompareOperator, bool> compare,
        Func<AnyVariableAndDataPair, string> desc,
        Action<AnyVariableAndDataPair, SetOperator> set)
        {
            _types.Add(variableType);
            _actions[variableType] = new AnyVariableAndDataPair.TypeActions(dataPropName, compare, desc, set);
        }
    }
}