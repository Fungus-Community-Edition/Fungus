using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Amanita.VScripting
{
    /// <summary>
    /// Maintains a registry of all available variables from various sources accessible in the scene.
    /// </summary>
    public sealed class VariableRegistry
    {
        private readonly AmanitaManager _manager;

        // Master dictionary of all variables
        private Dictionary<string, IVariable> _vars = new();

        // Secondary index: contentType -> dict of vars
        private Dictionary<Type, Dictionary<string, IVariable>> _varsByType = new();

        public IReadOnlyDictionary<string, IVariable> Variables => _vars;
        public event Action RegistryChanged;

        public VariableRegistry(AmanitaManager manager)
        {
            _manager = manager;
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
                }
            }

            void Register(string key, IVariable toRegister)
            {
                newVars[key] = toRegister;

                var type = toRegister.ContentType;
                newVarsByType.TryGetValue(type, out var dictForContentType);
                bool weHaveDictForThisContentType = dictForContentType != null;

                if (!weHaveDictForThisContentType)
                {
                    dictForContentType = new Dictionary<string, IVariable>();
                    newVarsByType[type] = dictForContentType;
                }
                dictForContentType[key] = toRegister;
            }

            // Other Flowcharts
            foreach (var otherChart in Flowchart.CachedFlowcharts.Where(fc => fc != localSource))
            {
                foreach (var toRegister in otherChart.Variables)
                {
                    string key = $"{otherChart.gameObject.name}/{toRegister.Key}";
                    Register(key, toRegister);
                }
            }

            // Globals
            foreach (var source in _manager.GlobalVariableSources)
            {
                if (source == null) continue;
                foreach (var toRegister in source.Variables)
                {
                    string key = $"~{source.name}~/{toRegister.Key}";
                    Register(key, toRegister);
                }
            }

            _vars = newVars;
            _varsByType = newVarsByType;
            RegistryChanged?.Invoke();
        }

        /// <summary>
        /// Returns variables matching any of the given content types.
        /// If null/empty, returns all.
        /// </summary>
        public IReadOnlyDictionary<string, IVariable> GetVarsOfTypes(Type[] contentTypes = null)
        {
            IReadOnlyDictionary<string, IVariable> result;
            bool giveThemEverything = contentTypes == null || contentTypes.Length == 0;
            if (giveThemEverything)
            {
                result = _vars;
            }
            else
            {
                if (contentTypes.Length == 1)
                {
                    var type = contentTypes[0];
                    if (_varsByType.TryGetValue(type, out var dict)) 
                    {
                        // This way, we don't make a whole new dictionary if we don't have to
                        result = dict;
                    }
                    else
                    {
                        result = emptyDict;
                    }
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
            }
            return result;
        }

        static readonly ReadOnlyDictionary<string, IVariable> emptyDict = 
            new ReadOnlyDictionary<string, IVariable>(new Dictionary<string, IVariable>());
    }
}