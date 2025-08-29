using System;
using UnityEditor;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;
using UnityEngine;

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

            // Clone UXML and anchor
            _rootElement = VariableDisplayEditorUxml.CloneTree();
            _rootElement.style.position = Position.Absolute;
            _rootElement.style.left = 10;
            _rootElement.style.bottom = 10;

            // Attach to FlowchartWindow's root
            _window.RootVisualElement.Add(_rootElement);

            // Build manager for current Flowchart
            BuildManager();

            DeregisterCallbacks();
            ListenForEvents();
        }

        protected void BuildManager()
        {
            var flowchart = _window?.Flowchart;
            if (flowchart == null)
                return;

            _manager?.Dispose();
            _manager = new VariableRowManager(_resolver);

            _manager.Init(new VRowManagerInitArgs
            {
                Root = _rootElement,
                ListContainer = _rootElement.Q<ScrollView>("rowList"),
                CountLabel = _rootElement.Q<UitkLabel>("varCountLabel"),
                AddButton = _rootElement.Q<Button>("addVarButton"),
                Flowchart = flowchart
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
            // Formerly built manager here in response to flowchart changes
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