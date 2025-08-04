using System;
using System.Collections.Generic;
using System.Linq;
using Type = System.Type;

namespace Amanita.VScripting.EditorUtils
{
    public class RowVisualHandlerResolver : IRowVisualHandlerResolver
    {
        public virtual Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType)
        {
            bool exactLookupSuccess = visualHandlerLookup.TryGetValue(contentType, out var handlerType);
            if (exactLookupSuccess)
            {
                return handlerType;
            }

            // 2. Inheritance match: pick the key with minimal “inheritance distance”
            var candidate = visualHandlerLookup.Keys
              .Where(baseType => baseType.IsAssignableFrom(contentType))
              .OrderBy(bt => InheritanceDistance(bt, contentType))
              .FirstOrDefault();

            if (candidate != null)
                return visualHandlerLookup[candidate];

            // 3. Explicit generic fallback
            return visualHandlerLookup.TryGetValue(typeof(object), out handlerType)
              ? handlerType
              : throw new InvalidOperationException(
                  $"No handler for {contentType.Name} and no generic fallback found.");

            // Helper: how many steps from baseType → derivedType
            static int InheritanceDistance(Type baseType, Type derivedType)
            {
                int distance = 0;
                for (var t = derivedType; t != null && t != baseType; t = t.BaseType)
                    distance++;
                return distance;
            }
        }
    }

    public interface IRowVisualHandlerResolver
    {
        Type ResolveHandler(IDictionary<Type, Type> visualHandlerLookup, Type contentType);
    }
}