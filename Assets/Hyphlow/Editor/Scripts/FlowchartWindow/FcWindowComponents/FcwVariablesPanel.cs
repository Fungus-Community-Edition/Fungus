using AtMycelia.EditorUtils;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    public sealed class FcwVariablesPanel : IFlowchartWindowModule, IFlowchartChangeResponder
    {
        public int Priority { get; set; } = 0;

        public void Initialize(FlowchartWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            _window = window;
            _isDisposed = false;

            _panelRoot = ResolveRootElement();
            if (_panelRoot == null)
            {
                Debug.LogError("[FcWindowVariablesPanelUitk] Could not find VarDisplay instance in UXML.");
                return;
            }

            BuildManager(_window.Flowchart);
            ToggleSubs(true);
            _panelRoot.BringToFront();
        }

        private FlowchartWindow _window;
        private bool _isDisposed;
        private VisualElement _panelRoot;

        private VisualElement ResolveRootElement()
        {
            return _window.rootVisualElement.Q<VisualElement>("VarDisplay");
        }

        private void BuildManager(Flowchart flowchart)
        {
            if (_isDisposed || _panelRoot == null || flowchart == null)
            {
                return;
            }

            _manager?.Dispose();
            _manager = new VariableRowManager();

            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            var handlerPool = new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            var rowPool = new VariableRowPool();

            _factoryInitArgs.Holder = _panelRoot;
            _factoryInitArgs.HandlerPool = handlerPool;
            _factoryInitArgs.RowPool = rowPool;
            _rowFactory.Init(_factoryInitArgs);

            var list = _panelRoot.Q<ListView>("rowList");
            var count = _panelRoot.Q<UitkLabel>("varCountLabel");
            var addBtn = _panelRoot.Q<Button>("addVarButton");

            var listViewArgs = new VariableListViewInitArgs
            {
                List = list,
                CountLabel = count,
                RowFactory = _rowFactory,
                VariableSource = flowchart,
                AssetResolver = new DefaultEditorAssetResolver(),
            };
            var view = new VariableListView(listViewArgs);

            _manager.Init(new VRowManagerInitArgs
            {
                Root = _panelRoot,
                AddButton = addBtn,
                VariableSource = flowchart,
                VariableListView = view,
            });
        }

        private VariableRowManager _manager;
        private IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();
        private readonly VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();
        private readonly VariableRowFactory _rowFactory = new VariableRowFactory();

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            }
            else
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            }
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                Flowchart fc = _window != null ? 
                    _window.Flowchart : 
                    null;
                BuildManager(fc);
            }
        }

        public void OnFlowchartChanged(Flowchart oldFc, Flowchart newFc)
        {
            BuildManager(newFc);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ToggleSubs(false);

            _manager?.Dispose();
            _manager = null;

            _window = null;
            _panelRoot = null;
            _resolver = null;
        }
    }
}