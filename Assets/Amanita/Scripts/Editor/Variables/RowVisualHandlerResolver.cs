using System;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;
using UnityEngine;

namespace Amanita.VScripting.EditorUtils
{
    public class RowVisualHandlerResolver : IRowVisualHandlerResolver
    {
        public virtual Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType)
        {
            Type handlerType;

            bool exactMatchFound = visualHandlerLookup.TryGetValue(contentType, out handlerType);
            if (exactMatchFound)
            {
                Debug.Log($"[Resolver] Exact match for {contentType.Name} → {handlerType.Name}");
                return handlerType;
            }

            // 2) Inheritance-based
            var candidates = visualHandlerLookup.Keys
                .Where(baseType => baseType.IsAssignableFrom(contentType))
                .Select(baseType => new 
                {
                    BaseType = baseType,
                    Distance = InheritanceDistance(baseType, contentType)
                })
                .OrderBy(x => x.Distance)
                .ToList();

            if (candidates.Any())
            {
                var closest = candidates.First();
                handlerType = visualHandlerLookup[closest.BaseType];
                Debug.Log($"[Resolver] Inherited match for {contentType.Name} → "
                    + $"{closest.BaseType.Name} ({closest.Distance} steps) → {handlerType.Name}");
                return handlerType;
            }

            // 3) Generic fallback
            if (visualHandlerLookup.TryGetValue(typeof(object), out handlerType))
            {
                Debug.Log($"[Resolver] Falling back for {contentType.Name} → {handlerType.Name}");
                return handlerType;
            }

            throw new InvalidOperationException(
                $"No handler for {contentType.Name} and no generic fallback found.");
        }

        // Helper: how many steps from baseType → derivedType
        static int InheritanceDistance(Type baseType, Type derivedType)
        {
            int distance = 0;
            for (var t = derivedType; t != null && t != baseType; t = t.BaseType)
                distance++;
            return distance;
        }
    }

    public interface IRowVisualHandlerResolver
    {
        Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType);
    }
}