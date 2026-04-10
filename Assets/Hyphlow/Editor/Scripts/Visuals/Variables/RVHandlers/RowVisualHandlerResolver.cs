using System;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class RowVisualHandlerResolver : IRowVisualHandlerResolver
    {
        public virtual Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType)
        {
            bool exactMatchFound = visualHandlerLookup.TryGetValue(contentType, out Type handlerType);
            if (exactMatchFound)
            {
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
                return handlerType;
            }

            // 3) Generic fallback
            if (visualHandlerLookup.TryGetValue(typeof(object), out handlerType))
            {
                return handlerType;
            }

            throw new InvalidOperationException(
                $"No handler for {contentType.Name} and no generic fallback found.");
        }

        // Helper: how many steps from baseType → derivedType
        static int InheritanceDistance(Type baseType, Type derivedType)
        {
            int distance = 0;
            for (var typeToCheck = derivedType;
                typeToCheck != null && typeToCheck != baseType;
                typeToCheck = typeToCheck.BaseType)
                distance++;
            return distance;
        }
    }

    public interface IRowVisualHandlerResolver
    {
        Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType);
    }
}