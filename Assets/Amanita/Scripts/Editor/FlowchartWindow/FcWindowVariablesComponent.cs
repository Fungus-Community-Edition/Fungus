using System;
using UnityEditor;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace Amanita.VScripting.EditorUtils
{
    public class FcWindowVariablesComponent : IFcWindowComponent, IDisposable
    {
        public VisualTreeAsset VariableDisplayEditorUxml { get; set; }

        protected FlowchartWindow window;
        protected TemplateContainer _rootElement;
        protected VariableRowManager _manager;
        protected IRowVisualHandlerResolver _resolver = new RowVisualHandlerResolver();

        public void Initialize(FlowchartWindow host)
        {
            window = host;

            // Clone UXML and anchor
            _rootElement = VariableDisplayEditorUxml.CloneTree();
            _rootElement.style.position = Position.Absolute;
            _rootElement.style.left = 10;
            _rootElement.style.bottom = 10;

            // Attach to FlowchartWindow's root
            window.rootVisualElement.Add(_rootElement);

            // Build manager for current Flowchart
            BuildManager();

            // Listen for play mode changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void BuildManager()
        {
            var flowchart = window?.Flowchart;
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

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                BuildManager();
            }
        }

        public void OnGUI(DrawBlockContext ctx, FlowchartContext fcCtx)
        {
            if (window.HandleFlowchartSelectionChange())
                BuildManager();
        }

        public void OnInspectorUpdate() { }
        public void OnEditorUpdate() { }
        public void OnToolbarGUI() { }
        public void OnInspectorGUI() { }

        public void Dispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

            _manager?.Dispose();
            _manager = null;

            _rootElement?.RemoveFromHierarchy();
            _rootElement = null;
        }
    }
}