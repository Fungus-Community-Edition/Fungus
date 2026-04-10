using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public sealed class CachedRowVisualTemplateProvider : IRowVisualTemplateProvider
    {
        public CachedRowVisualTemplateProvider()
        {
            _templateCache = new Dictionary<string, VisualTreeAsset>(StringComparer.Ordinal);
            _loggedMissingHandlers = new HashSet<Type>();
        }

        private readonly Dictionary<string, VisualTreeAsset> _templateCache;
        private readonly HashSet<Type> _loggedMissingHandlers; // For unit-testing purposes

        public VisualTreeAsset GetTemplate(Type handlerType)
        {
            string logMessage, cacheKey;
            if (!IsValid(handlerType))
            {
                return null;
            }
            bool IsValid(Type handlerType)
            {
                bool result = true;

                if (handlerType == null)
                {
                    logMessage = "CachedRowVisualTemplateProvider received a null handlerType when resolving templates.";
                    Debug.LogError(logMessage);
                    result = false;
                }
                else if (_loggedMissingHandlers.Contains(handlerType))
                {
                    result = false;
                }

                cacheKey = handlerType.AssemblyQualifiedName;
                if (string.IsNullOrEmpty(cacheKey))
                {
                    logMessage = "CachedRowVisualTemplateProvider could not compute a cache key for handlerType.";
                    Debug.LogError(logMessage);
                    result = false;
                }

                return result;
            }

            bool assetReady = _templateCache.TryGetValue(cacheKey, out VisualTreeAsset cachedAsset);
            if (assetReady && cachedAsset != null)
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

        private VisualTreeAsset ResolveTemplate(Type handlerType)
        {
            RowVisualHandlerAttribute attribute = handlerType.GetCustomAttribute<RowVisualHandlerAttribute>();
            string logMessage;
            if (attribute == null)
            {
                logMessage = $"{handlerType.Name} is missing RowVisualHandlerAttribute.";
                Debug.LogError(logMessage);
                _loggedMissingHandlers.Add(handlerType);
                return null;
            }

            VisualTreeAsset visualTreeAsset = Resources.Load<VisualTreeAsset>(attribute.PathToTemplate);

            if (visualTreeAsset == null)
            {
                logMessage = string.Format(MissingTemplateFormat, handlerType.Name,
                    attribute.PathToTemplate);
                Debug.LogError(logMessage);
                _loggedMissingHandlers.Add(handlerType);
                return null;
            }

            return visualTreeAsset;
        }

        private const string MissingTemplateFormat =
                "Template for {0} not found at '{1}'.\nPlease update the path in the " +
            "RowVisualHandlerAttribute of the former.";

        public void ClearCache()
        {
            _templateCache.Clear();
            _loggedMissingHandlers.Clear();
        }
    }
    public interface IRowVisualTemplateProvider
    {
        VisualTreeAsset GetTemplate(Type handlerType);
        void ClearCache();
    }

}