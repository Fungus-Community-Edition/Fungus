using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityObj = UnityEngine.Object;

#if UNITY_EDITOR
#endif

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Maintains a registry of all available variables from various sources accessible in the scene.
    /// </summary>
    public sealed class VariableRegistry
    {
        private readonly Func<IReadOnlyList<VariableSourceAsset>> _globalSourcesProvider;

        // Master dictionary of all variables
        private readonly Dictionary<string, IVariable> _vars = new Dictionary<string, IVariable>();

        // Secondary index: contentType -> dict of vars
        private readonly Dictionary<Type, Dictionary<string, IVariable>> _varsByType =
            new Dictionary<Type, Dictionary<string, IVariable>>();

        public IReadOnlyDictionary<string, IVariable> Variables => _vars;
        public event Action RegistryChanged;

        public VariableRegistry(Func<IReadOnlyList<VariableSourceAsset>> globalSourcesProvider)
        {
            _globalSourcesProvider = globalSourcesProvider ?? (() => emptySources);
            Rebuild();
#if UNITY_EDITOR
            ToggleEditorSubs(false);
            ToggleEditorSubs(true);
#endif
        }

        private void ToggleEditorSubs(bool on)
        {
            // No-op for now.
        }

        public void Rebuild(IVariableSource localSource = null)
        {
            _vars.Clear();
            _varsByType.Clear();

            RegisterLocalVars();
            void RegisterLocalVars()
            {
                if (localSource != null)
                {
                    foreach (var toRegister in localSource.Variables)
                    {
                        Register(toRegister.Key, toRegister);
                        bool isLegacyVariable = toRegister is Variable;
                        if (!isLegacyVariable)
                        {
                            toRegister.Owner = localSource;
                        }
                    }
                }
            }

            RegisterOtherFcVars();
            void RegisterOtherFcVars()
            {
                IReadOnlyList<Flowchart> cachedFcs = FindFlowchartsToGoThrough();
                IReadOnlyList<Flowchart> FindFlowchartsToGoThrough()
                {
                    IReadOnlyList<Flowchart> cachedFcs = FlowchartRegistry.GetFlowcharts()
                        .Where(fc => fc != null && !ReferenceEquals(fc, localSource)).ToArray();
                    return cachedFcs;
                }

                foreach (var otherFc in cachedFcs)
                {
                    foreach (var toRegister in otherFc.Variables)
                    {
                        if (toRegister.Scope != VariableScope.Public)
                        {
                            continue;
                        }

                        // To make it clear to users these variables are _not_ local to the source they're editing from,
                        // we prefix said vars with their owners' names based on a specific format.
                        string key = string.Format(_nonLocalFlowchartKeyFormat, otherFc.gameObject.name, toRegister.Key);
                        Register(key, toRegister);
                        bool isLegacyVariable = toRegister is Variable;
                        if (!isLegacyVariable)
                        {
                            toRegister.Owner = otherFc;
                        }
                    }
                }
            }

            RegisterGlobals();
            void RegisterGlobals()
            {
                IReadOnlyList<VariableSourceAsset> globalSources = _globalSourcesProvider()
                    .Where(source => source != null && source != localSource as UnityObj).ToArray();
                globalSources ??= emptySources;

                foreach (var source in globalSources)
                {
                    foreach (var toRegister in source.Variables)
                    {
                        string key = string.Format(_globalSourceKeyFormat, source.name, toRegister.Key);
                        Register(key, toRegister);
                        bool isLegacyVariable = toRegister is Variable;
                        if (!isLegacyVariable)
                        {
                            toRegister.Owner = source;
                        }
                    }
                }
            }

            RegistryChanged?.Invoke();
        }

        /// <summary>
        /// Registers the given variable under the given key, and also adds it to the secondary index for its content type.
        /// </summary>
        private void Register(string key, IVariable toRegister)
        {
            // The key we want to register the var under won't necessarily be the same as the
            // var's own key, since we might want to prefix or postfix it with something.
            _vars[key] = toRegister;

            var type = toRegister.ContentType;
            var dictForContentType = EnsureDictForContentType(type);
            dictForContentType[key] = toRegister;
        }

        private Dictionary<string, IVariable> EnsureDictForContentType(Type contentType)
        {
            _varsByType.TryGetValue(contentType, out var dictForContentType);
            bool weHaveDictForContentType = dictForContentType != null;
            if (!weHaveDictForContentType)
            {
                dictForContentType = new Dictionary<string, IVariable>();
                _varsByType[contentType] = dictForContentType;
            }
            return dictForContentType;
        }

        private static readonly string _nonLocalFlowchartKeyFormat = "[{0}]/{1}";
        private static readonly string _globalSourceKeyFormat = "~{0}~/{1}";

        /// <summary>
        /// Returns available variables matching the given content type. If getAllAssignableTypes 
        /// is true, it also returns variables whose content types are assignable to the given 
        /// content type (e.g. if contentType is Component, it also returns variables of type 
        /// SpriteRenderer since SpriteRenderer is a Component).
        /// </summary>
        public IReadOnlyDictionary<string, IVariable> GetVarsOfType(Type contentType = null, 
            bool getAllAssignableTypes = false)
        {
            IReadOnlyDictionary<string, IVariable> result;
            bool giveThemEverything = contentType == null;
            if (giveThemEverything)
            {
                result = _vars;
            }
            else
            {
                if (getAllAssignableTypes)
                {
                    var merged = new Dictionary<string, IVariable>();
                    foreach (var kvp in _varsByType)
                    {
                        var type = kvp.Key;
                        bool compatible = TypeUtils.TypesCompatible(contentType, type);
                        if (compatible)
                        {
                            foreach (var kvp2 in kvp.Value)
                            {
                                merged[kvp2.Key] = kvp2.Value;
                            }
                        }
                    }
                    result = merged;
                }
                else if (_varsByType.TryGetValue(contentType, out var dict))
                {
                    // This way, we don't make a whole new dictionary if we don't have to
                    result = dict;
                }
                else
                {
                    result = emptyDict;
                }

            }
            return result;
        }

        /// <summary>
        /// Returns available variables matching any of the given content types.
        /// If null/empty, returns all.
        /// </summary>
        public IReadOnlyDictionary<string, IVariable> GetVarsOfMultiTypes(IList<Type> contentTypes = null, 
            bool getAllAssignableTypes = false)
        {
            IReadOnlyDictionary<string, IVariable> result;
            bool giveThemEverything = contentTypes == null || contentTypes.Count == 0;
            if (giveThemEverything)
            {
                result = _vars;
            }
            else if (contentTypes.Count == 1)
            {
                return GetVarsOfType(contentTypes[0], getAllAssignableTypes);
            }
            else
            {
                var merged = new Dictionary<string, IVariable>();
                if (getAllAssignableTypes)
                {
                    foreach (var kvp in _varsByType)
                    {
                        var type = kvp.Key;
                        bool compatible = false;
                        for (int i = 0; i < contentTypes.Count; i++)
                        {
                            if (TypeUtils.TypesCompatible(contentTypes[i], type))
                            {
                                compatible = true;
                                break;
                            }
                        }

                        if (compatible)
                        {
                            foreach (var kvp2 in kvp.Value)
                            {
                                merged[kvp2.Key] = kvp2.Value;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < contentTypes.Count; i++)
                    {
                        var type = contentTypes[i];
                        if (_varsByType.TryGetValue(type, out var dict))
                        {
                            foreach (var kvp in dict)
                            {
                                merged[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                }

                result = merged;

            }
            return result;
        }

        private static readonly IReadOnlyList<VariableSourceAsset> emptySources = new List<VariableSourceAsset>();
        private static readonly ReadOnlyDictionary<string, IVariable> emptyDict =
            new ReadOnlyDictionary<string, IVariable>(new Dictionary<string, IVariable>());
    }
}