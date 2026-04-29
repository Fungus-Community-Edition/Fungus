using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using System.Linq;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public static class RowVisualHandlerRegistry
    {
        [InitializeOnLoadMethod]
        private static void InitializeHandlerLookup()
        {
            // Always rebuild lookup right now
            RefreshHandlerLookup(); 

            // Ensure we only subscribe once
            AssemblyReloadEvents.afterAssemblyReload -= RefreshHandlerLookup;
            AssemblyReloadEvents.afterAssemblyReload += RefreshHandlerLookup;
        }

        private static readonly object _handlerLookupLock = new object();

        public static void RefreshHandlerLookup()
        {
            var visHandlerType = typeof(RowVisualHandler);

            // 1) Snapshot types safely (avoid ReflectionTypeLoadException)
            var discovered = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .Where(typeEl =>
                    visHandlerType.IsAssignableFrom(typeEl) &&
                    !typeEl.IsAbstract &&
                    typeEl.GetCustomAttribute<RowVisualHandlerAttribute>() != null)
                .ToArray(); // snapshot

            // 2) Precompute the pairs outside the lock //
            var pairs = new List<KeyValuePair<Type, Type>>(discovered.Length);
            foreach (var handlerType in discovered)
            {
                var attr = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
                pairs.Add(new KeyValuePair<Type, Type>(attr.ContentType, handlerType));
            }

            // 3) Atomic update under the lock (keep same dictionary instance)
            lock (_handlerLookupLock)
            {
                _visualHandlerLookup.Clear();
                foreach (var kv in pairs)
                    _visualHandlerLookup[kv.Key] = kv.Value;

                allVisualHandlerTypes = discovered; // consistent with the lookup now
            }
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException e) { return e.Types.Where(typeEl => typeEl != null); }
        }

        private static IEnumerable<Type> allVisualHandlerTypes;

        public static IDictionary<Type, Type> VisualHandlerLookup
        {
            get {  return new Dictionary<Type, Type>(_visualHandlerLookup); }
        }
        // Keys are var content types (like for floats, ints, etc), values are the types
        // of the visual handlers meant for the corresponding keys
        private static readonly IDictionary<Type, Type> _visualHandlerLookup = new Dictionary<Type, Type>(new TypeNameComparer());

    }
}