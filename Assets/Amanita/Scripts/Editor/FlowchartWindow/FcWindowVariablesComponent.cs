using Amanita.EditorUtils;
using System;
using UnityEditor;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowVariablesComponent : IFcWindowComponent, IDisposable
    {
        public VisualTreeAsset VariableDisplayEditorUxml { get; set; }

        protected IFlowchartHost _window;
        protected TemplateContainer _rootElement;
        protected VariableRowManager _manager;
        protected IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();

        public void Initialize(IFlowchartHost host)
        {
            _window = host;

            _rootElement = VariableDisplayEditorUxml.CloneTree();
            _rootElement.style.position = Position.Absolute;
            _rootElement.style.left = 10;
            _rootElement.style.bottom = 10;

            _window.RootVisualElement.Add(_rootElement);

            BuildManager();

            DeregisterCallbacks();
            ListenForEvents();
        }

        protected VariableRowFactory _rowFactory = new VariableRowFactory();
        protected VariableRowFactoryInitArgs _factoryInitArgs = new VariableRowFactoryInitArgs();

        protected void BuildManager()
        {
            var flowchart = _window?.Flowchart;
            if (flowchart == null)
                return;

            _manager?.Dispose();
            _manager = new VariableRowManager();

            var visualHandlerLookup = RowVisualHandlerRegistry.VisualHandlerLookup;
            var handlerPool = new RowVisualHandlerPool(_resolver, visualHandlerLookup);
            var rowPool = new VariableRowPool();
            var holder = _rootElement;

            _factoryInitArgs.Holder = holder;
            _factoryInitArgs.HandlerPool = handlerPool;
            _factoryInitArgs.RowPool = rowPool;
            _rowFactory.Init(_factoryInitArgs);

            var list = _rootElement.Q<ListView>("rowList");
            var count = _rootElement.Q<UitkLabel>("varCountLabel");
            var addBtn = _rootElement.Q<Button>("addVarButton");

            var listViewArgs = new VariableListViewInitArgs()
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
                Root = _rootElement,
                AddButton = addBtn,
                VariableSource = flowchart,
                VariableListView = view,
            });
        }

        protected virtual void DeregisterCallbacks()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            FlowchartWindowSignals.ChangedFlowchart -= OnFlowchartChanged;
        }

        protected void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                BuildManager();
            }
        }

        protected virtual void OnFlowchartChanged(Flowchart prevFlowchar, Flowchart newFlowchart)
        {
            BuildManager();
        }

        protected virtual void ListenForEvents()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            FlowchartWindowSignals.ChangedFlowchart += OnFlowchartChanged;
        }

        public void OnGUI(DrawBlockContext ctx, FlowchartContext fcCtx)
        {
            // Variable list is now entirely UIToolkit/virtualized; no GUI draw needed.
        }

        public void OnInspectorUpdate() { }
        public void OnEditorUpdate() { }
        public void OnToolbarGUI() { }
        public void OnInspectorGUI() { }

        public void Dispose()
        {
            DeregisterCallbacks();

            _window = null;
            _resolver = null;

            _manager?.Dispose();
            _manager = null;

            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
        }
    }
}