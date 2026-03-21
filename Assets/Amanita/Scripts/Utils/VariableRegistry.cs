using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Amanita.VScripting
{
    /// <summary>
    /// Maintains a registry of all available variables from various sources accessible in the scene.
    /// </summary>
    public sealed class VariableRegistry
    {
        private readonly Func<IReadOnlyList<VariableSourceAsset>> _globalSourcesProvider;

        // Master dictionary of all variables
        private Dictionary<string, IVariable> _vars = new Dictionary<string, IVariable>();

        // Secondary index: contentType -> dict of vars
        private Dictionary<Type, Dictionary<string, IVariable>> _varsByType =
            new Dictionary<Type, Dictionary<string, IVariable>>();

        public IReadOnlyDictionary<string, IVariable> Variables => _vars;
        public event Action RegistryChanged;

        public VariableRegistry(Func<IReadOnlyList<VariableSourceAsset>> globalSourcesProvider)
        {
            _globalSourcesProvider = globalSourcesProvider ?? (() => emptySources);
            Rebuild();
#if UNITY_EDITOR
            Selection.selectionChanged += OnSelectionChanged;
#endif
        }

#if UNITY_EDITOR
        private void OnSelectionChanged()
        {
            var selected = Selection.activeGameObject;
            if (selected != null && selected.TryGetComponent<Flowchart>(out var fc))
            {
                Rebuild(fc);
            }
        }
#endif

        public void Rebuild(IVariableSource localSource = null)
        {
            var newVars = new Dictionary<string, IVariable>();
            var newVarsByType = new Dictionary<Type, Dictionary<string, IVariable>>();

            // Local
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

            void Register(string key, IVariable toRegister)
            {
                newVars[key] = toRegister;

                var type = toRegister.ContentType;
                newVarsByType.TryGetValue(type, out var dictForContentType);
                bool weHaveDictForContentType = dictForContentType != null;

                if (!weHaveDictForContentType)
                {
                    dictForContentType = new Dictionary<string, IVariable>();
                    newVarsByType[type] = dictForContentType;
                }
                dictForContentType[key] = toRegister;
            }

            // Other Flowcharts
            var amanitaManager = AmanitaManager.S;
            IReadOnlyList<Flowchart> cachedFcs = amanitaManager != null && amanitaManager.FlowchartsInScene != null
                ? amanitaManager.FlowchartsInScene
                : Array.Empty<Flowchart>();

            foreach (var otherChart in cachedFcs.Where(fc => fc != null && !ReferenceEquals(fc, localSource)))
            {
                foreach (var toRegister in otherChart.Variables)
                {
                    if (toRegister == null)
                    {
                        continue;
                    }
                    string key = $"{otherChart.gameObject.name}/{toRegister.Key}";
                    Register(key, toRegister);
                    bool isLegacyVariable = toRegister is Variable;
                    if (!isLegacyVariable)
                    {
                        toRegister.Owner = otherChart;
                    }
                }
            }

            // Globals
            IReadOnlyList<VariableSourceAsset> globalSources = _globalSourcesProvider();
            if (globalSources == null)
            {
                globalSources = emptySources;
            }

            foreach (var source in globalSources)
            {
                if (source == null) continue;
                foreach (var toRegister in source.Variables)
                {
                    string key = $"~{source.name}~/{toRegister.Key}";
                    Register(key, toRegister);
                    bool isLegacyVariable = toRegister is Variable;
                    if (!isLegacyVariable)
                    {
                        toRegister.Owner = source;
                    }
                }
            }

            _vars = newVars;
            _varsByType = newVarsByType;
            RegistryChanged?.Invoke();
        }

        public IReadOnlyDictionary<string, IVariable> GetVarsOfType(Type contentType = null)
        {
            IReadOnlyDictionary<string, IVariable> result;
            bool giveThemEverything = contentType == null;
            if (giveThemEverything)
            {
                result = _vars;
            }
            else
            {
                if (_varsByType.TryGetValue(contentType, out var dict))
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
        public IReadOnlyDictionary<string, IVariable> GetVarsOfMultiTypes(Type[] contentTypes = null)
        {
            IReadOnlyDictionary<string, IVariable> result;
            bool giveThemEverything = contentTypes == null || contentTypes.Length == 0;
            if (giveThemEverything)
            {
                result = _vars;
            }
            else if (contentTypes.Length == 1)
            {
                return GetVarsOfType(contentTypes[0]);
            }
            else
            {
                var merged = new Dictionary<string, IVariable>();
                for (int i = 0; i < contentTypes.Length; i++)
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
                result = merged;

            }
            return result;
        }

        private static readonly IReadOnlyList<VariableSourceAsset> emptySources = new List<VariableSourceAsset>();
        private static readonly ReadOnlyDictionary<string, IVariable> emptyDict =
            new ReadOnlyDictionary<string, IVariable>(new Dictionary<string, IVariable>());
    }
}