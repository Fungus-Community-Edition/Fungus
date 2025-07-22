using System;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    internal class FcWindowVariablesComponent : IFcWindowComponent, IDisposable
    {
        public virtual void Initialize(FlowchartWindow host)
        {
            window = host;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            BuildAdapter();
            BuildUI();
        }
        
        protected FlowchartWindow window;

        protected virtual void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
            {
                DisposeAdapter();
                BuildAdapter();
                BuildUI();
            }
        }

        protected virtual void BuildAdapter()
        {
            var flowchart = window.Flowchart;
            bool noSelectedFlowchartChange = uitkAdapter != null && uitkAdapter.TargetFlowchart == flowchart;
            if (flowchart == null || noSelectedFlowchartChange)
            {
                DisposeAdapter();
                return;
            }

            DisposeAdapter();

            // Wrap the Flowchart in a SerializedObject to drive PropertyFields
            flowchartSO = new SerializedObject(flowchart);
            variablesProp = flowchartSO.FindProperty("variables");
            uitkAdapter = new UitkVariableListAdaptor(variablesProp, flowchart);
        }

        protected SerializedObject flowchartSO;
        protected SerializedProperty variablesProp;
        protected UitkVariableListAdaptor uitkAdapter;

        // Construct the UIElements container and insert into the window
        protected virtual void BuildUI()
        {
            container?.RemoveFromHierarchy();

            container = new VisualElement { name = "variables-container" };

            SetContainerStyling();
            void SetContainerStyling()
            {
                // We want it at the bottom left corner of the FcC Window, wide
                // enough to give all the fields a decent amount of space. 
                // Unlike the orig var adapter, we want the height to be fixed
                // so the devs can scroll through the vars without too much
                // of the window space monopolized. All regardless of how many
                // variables an FC has.
                container.style.position = Position.Absolute;
                container.style.left = 10;
                container.style.bottom = 10;

                // Dimensions
                container.style.width = Width;
                container.style.height = Height;
            }

            if (uitkAdapter != null)
            {
                VisualElement variablesUI = uitkAdapter.CreateVariablesUI();
                container.Add(variablesUI);
            }

            window.rootVisualElement.Add(container);
        }

        protected VisualElement container;

        public virtual int Width { get; set; } = 450;
        public virtual int Height { get; set; } = 200;

        // To ensure the adapter stays in sync.
        public virtual void OnGUI(DrawBlockContext ctx, FlowchartContext fcCtx)
        {
            bool selectionChanged = window.HandleFlowchartSelectionChange();
            if (selectionChanged)
            {
                BuildAdapter();
                BuildUI();
            }
        }

        private int lastVarHash = 0;

        public virtual void OnInspectorUpdate()
        {
            flowchartSO?.Update();
        }

        public virtual void OnEditorUpdate()
        {
            if (uitkAdapter == null) return;
            int currentHash = uitkAdapter.VarsList.Aggregate(0, (acc, v) => acc ^ (v?.GetHashCode() ?? 0));
            if (currentHash != lastVarHash)
            {
                uitkAdapter.RefreshListView();
                lastVarHash = currentHash;
            }
        }

        #region No-ops

        public virtual void OnToolbarGUI()
        {
        }

        public virtual void OnInspectorGUI()
        {
        }
        #endregion

        #region IDisposable

        protected virtual void DisposeAdapter()
        {
            if (uitkAdapter != null)
            {
                uitkAdapter.Dispose();
                uitkAdapter = null;
            }
            flowchartSO = null;
            variablesProp = null;
        }

        public virtual void Dispose()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            DisposeAdapter();
            container?.RemoveFromHierarchy();
            container = null;
        }

        #endregion
    }
}