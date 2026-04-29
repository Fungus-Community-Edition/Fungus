using System;
using System.Collections.Generic;
using AtMycelia.Hyphlow.Sys;
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

        private readonly Func<IReadOnlyList<VariableSourceAsset>> _globalSourcesProvider;
        private readonly VariableRegistryConfig _config;
        private readonly VariableRegistry _registry;

        public VariableRegistry LocalRegistry => _registry;

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                VsaSignals.VsaEnabled += OnVsaChanged;
                VsaSignals.VsaDisabled += OnVsaChanged;
                VsaSignals.VariableAdded += OnAnyVariableChanged;
                VsaSignals.VariableRemoved += OnAnyVariableChanged;

                VariableSignals.PostValueChange += OnVariableValueChanged;

                FlowchartSignals.FlowchartDestroyed += OnFlowchartDestroyed;
                FlowchartSignals.VariableAdded += OnVarAdded;
                FlowchartSignals.VariableRemoved += OnVarRemoved;

                // No need to listen for the editor opening or closing a scene. The FlowchartRegistry
                // will full refresh itself in response to that, and then in response to that full 
                // refresh, we'll make the VariableRegistry full refresh as well.
                FlowchartRegistry.FullRefreshed += OnFcRegFullRefreshed;

#if UNITY_EDITOR
                Selection.selectionChanged += OnSelectionChanged;
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
                VsaSignals.VariableAdded -= OnAnyVariableChanged;
                VsaSignals.VariableRemoved -= OnAnyVariableChanged;

                FlowchartRegistry.FullRefreshed -= OnFcRegFullRefreshed;

                VariableSignals.PostValueChange -= OnVariableValueChanged;

                FlowchartSignals.FlowchartDestroyed -= OnFlowchartDestroyed;
                FlowchartSignals.VariableAdded -= OnVarAdded;
                FlowchartSignals.VariableRemoved -= OnVarRemoved;

#if UNITY_EDITOR
                Selection.selectionChanged -= OnSelectionChanged;
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
#endif

                if (_config != null)
                {
                    _config.Changed -= OnConfigChanged;
                }
            }
        }

        private void OnFcRegFullRefreshed()
        {
            RebuildAll();
        }

        private void OnFlowchartDestroyed(Flowchart flowchart)
        {
            _registry.Rebuild();
        }

        private void OnVsaChanged(VariableSourceAsset asset)
        {
            _registry.Rebuild();
        }

        private void OnAnyVariableChanged(VariableSourceAsset _, IVariable _2)
        {
            _registry.Rebuild();
        }

        private void OnVariableValueChanged(IVariable variable, object arg2)
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                EditorApplication.delayCall += () =>
                {
                    if (variable == null)
                    {
                        return;
                    }
                    if (Application.isPlaying)
                    {
                        return; // We only want to respond to var value changes in the editor,
                                // since that's the only time we care about keeping the registry's
                                // values up to date with the actual variable values in the scene.
                    }
                    OnSelectionChanged();
                };
#endif
            }
            else
            {
                _registry.Rebuild();
            }
        }

        private void OnSelectionChanged()
        {
#if UNITY_EDITOR
            var selected = Selection.activeGameObject;
            
            
            if (selected != null && selected.TryGetComponent<Flowchart>(out var fc))
            {
                Rebuild(fc);
            }
#endif
        }

        private void OnVarAdded(Flowchart flowchart, IVariable _)
        {
            _registry.Rebuild(flowchart);
        }

        private void OnVarRemoved(Flowchart flowchart, IVariable _)
        {
            _registry.Rebuild(flowchart);
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
            {
                _registry.Rebuild();
            }
        }

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
            HyphlowRuntimeSysAssets.EnsureExists();
            if (HyphlowRuntimeSysAssets.S == null)
            {
                return null;
            }
            if (HyphlowRuntimeSysAssets.S != null && HyphlowRuntimeSysAssets.S.VariableRegistryConfig == null)
            {
                HyphlowRuntimeSysAssets.S.VariableRegistryConfig =
                    Resources.Load<VariableRegistryConfig>(DefaultConfigResourcesPath);
            }

            return HyphlowRuntimeSysAssets.S.VariableRegistryConfig;
        }

        private void OnConfigChanged()
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
            var reg = EnsureDefault();
            reg.Rebuild(localSource);
        }
    }
}