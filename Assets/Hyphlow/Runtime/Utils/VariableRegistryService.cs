using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// Singleton service that maintains a variable registry and handles its updates in response to 
    /// changes in the variable sources.
    /// </summary>
    public sealed class VariableRegistryService : IDisposable
    {
        private readonly Func<IReadOnlyList<VariableSourceAsset>> _globalSourcesProvider;
        private readonly VariableRegistryConfig _config;
        private readonly VariableRegistry _registry;

        public VariableRegistryService(Func<IReadOnlyList<VariableSourceAsset>> globalSourcesProvider,
            VariableRegistryConfig config)
        {
            if (globalSourcesProvider == null)
            {
                throw new ArgumentNullException(nameof(globalSourcesProvider));
            }

            _globalSourcesProvider = globalSourcesProvider;
            _config = config;
            _registry = new VariableRegistry(_globalSourcesProvider);

            ToggleSubs(true);
        }

        public VariableRegistry LocalRegistry => _registry;

        public void Rebuild(IVariableSource localSource = null)
        {
            _registry.Rebuild(localSource);
        }

        public void Dispose()
        {
            ToggleSubs(false);
        }

        public static VariableRegistryService Current { get; private set; }

        public static VariableRegistryService EnsureDefault()
        {
            if (Current != null)
            {
                return Current;
            }

            VariableRegistryConfig config = LoadDefaultConfig();
            Func<IReadOnlyList<VariableSourceAsset>> provider = () => config != null ? 
            config.GlobalSources : 
            emptySources;

            VariableRegistryService service = new VariableRegistryService(provider, config);
            SetCurrent(service);
            return service;
        }

        public static void SetCurrent(VariableRegistryService service)
        {
            if (ReferenceEquals(Current, service))
            {
                return;
            }

            if (Current != null)
            {
                Current.Dispose();
            }

            Current = service;
        }

        public static void ClearCurrent(VariableRegistryService service)
        {
            if (!ReferenceEquals(Current, service))
            {
                return;
            }

            Current.Dispose();
            Current = null;
        }

        public static VariableRegistryConfig LoadDefaultConfig()
        {
            if (DefaultHyphlowAssets.VariableRegistryConfig == null)
            {
                DefaultHyphlowAssets.VariableRegistryConfig =
                    Resources.Load<VariableRegistryConfig>(DefaultConfigResourcesPath);
            }

            return DefaultHyphlowAssets.VariableRegistryConfig;
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                VsaSignals.VsaEnabled += OnVsaChanged;
                VsaSignals.VsaDisabled += OnVsaChanged;

#if UNITY_EDITOR
                FlowchartSignals.VariableAdded += OnVarAdded;
                FlowchartSignals.VariableRemoved += OnVarRemoved;

                VariableSourceAsset.AnyRightBeforeVarAdded += OnAnyVariableChanged;
                VariableSourceAsset.AnyRightBeforeVarRemoved += OnAnyVariableChanged;

                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif

                if (_config != null)
                {
                    _config.Changed += OnConfigChanged;
                }
            }
            else
            {
                VsaSignals.VsaEnabled -= OnVsaChanged;
                VsaSignals.VsaDisabled -= OnVsaChanged;

#if UNITY_EDITOR
                FlowchartSignals.VariableAdded -= OnVarAdded;
                FlowchartSignals.VariableRemoved -= OnVarRemoved;

                VariableSourceAsset.AnyRightBeforeVarAdded -= OnAnyVariableChanged;
                VariableSourceAsset.AnyRightBeforeVarRemoved -= OnAnyVariableChanged;

                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

                if (_config != null)
                {
                    _config.Changed -= OnConfigChanged;
                }
            }
        }

        private void OnVarRemoved(Flowchart flowchart, IVariable variable)
        {
            _registry.Rebuild(flowchart);
        }

        private void OnVarAdded(Flowchart flowchart, IVariable variable)
        {
            _registry.Rebuild(flowchart);
        }

        private void OnConfigChanged()
        {
            _registry.Rebuild();
        }

        private void OnVsaChanged(VariableSourceAsset asset)
        {
            _registry.Rebuild();
        }

        private void OnAnyVariableChanged(Muscariable variable)
        {
            _registry.Rebuild();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            _registry.Rebuild();
        }

        private static readonly IReadOnlyList<VariableSourceAsset> emptySources = new List<VariableSourceAsset>();
        private const string DefaultConfigResourcesPath = "AtMycelia/Amanita/VariableRegistryConfig";

        public static VariableRegistry Registry
        {
            get
            {
                return EnsureDefault()._registry;
            }
        }

        public static IReadOnlyList<VariableSourceAsset> GlobalSources
        {
            get
            {
                VariableRegistryConfig config = LoadDefaultConfig();
                if (config != null)
                {
                    return config.GlobalSources;
                }

                return emptySources;
            }
        }

        public static IReadOnlyList<IVariable> GlobalVariables
        {
            get
            {
                List<IVariable> result = new List<IVariable>();
                IReadOnlyList<VariableSourceAsset> sources = GlobalSources;
                for (int i = 0; i < sources.Count; i++)
                {
                    VariableSourceAsset source = sources[i];
                    if (source == null)
                    {
                        continue;
                    }

                    IReadOnlyList<IVariable> vars = source.Variables;
                    for (int j = 0; j < vars.Count; j++)
                    {
                        IVariable varEl = vars[j];
                        if (varEl != null)
                        {
                            result.Add(varEl);
                        }
                    }
                }

                return result;
            }
        }

        public static void RebuildAll(IVariableSource localSource = null)
        {
            EnsureDefault().Rebuild(localSource);
        }
    }
}