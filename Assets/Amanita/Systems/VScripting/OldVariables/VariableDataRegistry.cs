using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amanita.VScripting
{
    public static class VariableDataRegistry
    {
        private static readonly Dictionary<Type, VariableDataMetadata> _registry = new();

        // Legacy fallback list reference (inject or link to your existing static list)
        public static Func<IEnumerable<Type>> LegacyListProvider { get; set; }

        public static void DiscoverAndRegisterAll(Assembly assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();

            var validVarDataTypes = assembly.GetTypes()
                .Where(typeFound => typeof(VariableData).
                IsAssignableFrom(typeFound) && !typeFound.IsAbstract && !typeFound.IsInterface);

            foreach (var typeEl in validVarDataTypes)
                Register(typeEl);
        }

        public static void Register<T>() where T : class
            => Register(typeof(T));

        public static void Register(Type varDataType)
        {
            if (!_registry.ContainsKey(varDataType))
            {
                var attr = varDataType.GetCustomAttribute<VariableDataAttribute>();

                // We want the attribute to be optional, and thus when the data type doesn't
                // have it, we go with some defaults.
                var displayName = attr?.DisplayName ?? varDataType.Name;
                var category = attr?.Category ?? "Uncategorized";

                _registry[varDataType] = new VariableDataMetadata(varDataType, displayName, category);
            }
        }

        public static VariableDataMetadata GetMetadata(Type type)
        {
            VariableDataMetadata result;
            _registry.TryGetValue(type, out result);

            // Fallback to legacy list if provided
            if (result == null && LegacyListProvider != null && LegacyListProvider().Contains(type))
                return new VariableDataMetadata(type, type.Name, "Legacy");

            return result;
        }

        public static IEnumerable<VariableDataMetadata> AllMetadata
        {
            get
            {
                var all = _registry.Values.ToList();

                if (LegacyListProvider != null)
                {
                    foreach (var legacyType in LegacyListProvider())
                    {
                        if (!_registry.ContainsKey(legacyType))
                            all.Add(new VariableDataMetadata(legacyType, legacyType.Name, "Legacy"));
                    }
                }

                return all;
            }
        }
    }

    public class VariableDataMetadata
    {
        public Type Type { get; }
        public string DisplayName { get; }
        public string Category { get; }

        public VariableDataMetadata(Type type, string displayName, string category)
        {
            Type = type;
            DisplayName = displayName;
            Category = category;
        }
    }

    
}