using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

namespace Amanita.VScripting
{
    public static class MuscariableFactory
    {
        private static readonly Dictionary<Type, Type> typeMap = new(new TypeNameComparer());
        private static readonly Type s_genericFallback = typeof(GenericMuscariable);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        [UnityEditor.InitializeOnLoadMethod]
        private static void BuildCache()
        {
            typeMap.Clear();
            var attrType = typeof(MuscariableAttribute);
            var baseType = typeof(Muscariable);

            var muscariableSubtypes = AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(SafeGetTypes)
                         .Where(typeToCheck => !typeToCheck.IsAbstract && baseType.IsAssignableFrom(typeToCheck));

            foreach (var typeFound in muscariableSubtypes)
            {
                var attr = typeFound.GetCustomAttribute<MuscariableAttribute>();
                if (attr == null)
                {
                    continue;
                }

                var contentType = attr.ContentType;
                // First come, first serve unless this is the generic fallback
                if (!typeMap.ContainsKey(contentType) || typeMap[contentType] == s_genericFallback)
                    typeMap[contentType] = typeFound;
            }

            bool fallbackReady = typeMap.ContainsKey(typeof(object));
            if (!fallbackReady)
                typeMap[typeof(object)] = s_genericFallback;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch { return Array.Empty<Type>(); }
        }

        public static Muscariable Create(Type contentType, IVariable src = null)
        {
            var baseObjType = typeof(object);
            if (contentType == null)
            {
                contentType = baseObjType;
            }

            if (!typeMap.TryGetValue(contentType, out var implType))
            {
                // No explicit mapping; use Muscariable<T> variant
                if (contentType.Equals(baseObjType)) // Need to avoid reference-checking with ==
                    implType = s_genericFallback;
                else
                    implType = typeof(GenericMuscariable);
            }

            var result = (Muscariable) Activator.CreateInstance(implType);

            SetFromSourceVar();
            void SetFromSourceVar()
            {
                if (src != null)
                {
                    result.Key = src.Key;
                    result.Scope = src.Scope;
                    result.ItemID = src.ItemID;
                    if (src.Value == null || result.ContentType.IsInstanceOfType(src.Value))
                        result.Value = src.Value;
                }
            }

            return result;
        }
    }
}