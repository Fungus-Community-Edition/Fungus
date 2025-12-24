using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    public sealed class CachedRowVisualTemplateProvider : IRowVisualTemplateProvider
    {
        private readonly Dictionary<string, VisualTreeAsset> _templateCache;
        private readonly HashSet<Type> _loggedMissingHandlers;

        private const string MissingTemplateFormat =
            "Template for {0} not found at '{1}'.\nPlease update the path in the RowVisualHandlerAttribute of the former.";

        public CachedRowVisualTemplateProvider()
        {
            _templateCache = new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);
            _loggedMissingHandlers = new HashSet<Type>();
        }

        public VisualTreeAsset GetTemplate(Type handlerType)
        {
            if (handlerType == null)
            {
                Debug.LogError("CachedRowVisualTemplateProvider needs a non-null handlerType.");
                return null;
            }

            if (_loggedMissingHandlers.Contains(handlerType))
            {
                return null;
            }

            string cacheKey = handlerType.AssemblyQualifiedName;
            if (string.IsNullOrEmpty(cacheKey))
            {
                Debug.LogError("CachedRowVisualTemplateProvider could not compute a cache key for handlerType.");
                return null;
            }

            if (_templateCache.TryGetValue(cacheKey, out VisualTreeAsset cachedAsset) && cachedAsset != null)
            {
                return cachedAsset;
            }

            VisualTreeAsset resolvedAsset = ResolveTemplate(handlerType);
            if (resolvedAsset != null)
            {
                _templateCache[cacheKey] = resolvedAsset;
            }

            return resolvedAsset;
        }

        public void ClearCache()
        {
            _templateCache.Clear();
            _loggedMissingHandlers.Clear();
        }

        private VisualTreeAsset ResolveTemplate(Type handlerType)
        {
            RowVisualHandlerAttribute attribute = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
            if (attribute == null)
            {
                Debug.LogError($"{handlerType.Name} is missing RowVisualHandlerAttribute.");
                _loggedMissingHandlers.Add(handlerType);
                return null;
            }

            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>(attribute.PathToTemplate);

            if (visualTreeAsset == null)
            {
                Debug.LogError(string.Format(
                    MissingTemplateFormat,
                    handlerType.Name,
                    attribute.PathToTemplate));
                _loggedMissingHandlers.Add(handlerType);
                return null;
            }

            return visualTreeAsset;
        }
    }

    public interface IRowVisualTemplateProvider
    {
        VisualTreeAsset GetTemplate(Type handlerType);
        void ClearCache();
    }

}