using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils
{
    internal class FcWindowVariablesComponent : IFcWindowComponent, IDisposable
    {
        public virtual void Initialize(FlowchartWindow host)
        {
            window = host;
            BuildAdapter();
            BuildUI();
        }

        protected FlowchartWindow window;

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

        public virtual void OnInspectorUpdate()
        {
            flowchartSO?.Update();
        }

        #region No-ops

        public virtual void OnEditorUpdate()
        {
            // Nothing needed here, since the adapter wires into Undo/Flowchart events itself
        }

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
            DisposeAdapter();
            container?.RemoveFromHierarchy();
            container = null;
        }

        #endregion
    }
}