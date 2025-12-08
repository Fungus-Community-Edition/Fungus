using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Amanita.VScripting.EditorUtils.Tests
{
    // A tiny stand-in for your Flowchart data model.
    // Rename members to match your real Flowchart API (events + variable accessors).
    internal class FakeFlowchart
    {
        public event Action VariablesChanged;

        public readonly List<string> Variables = new();

        public void AddVariable(string name = null)
        {
            Variables.Add(name ?? $"var_{Variables.Count}");
            VariablesChanged?.Invoke();
        }

        public void RemoveLast()
        {
            if (Variables.Count == 0) return;
            Variables.RemoveAt(Variables.Count - 1);
            VariablesChanged?.Invoke();
        }
    }

    // Minimal FlowchartWindow-like shim for the harness
    internal class VariablesPanelHarnessWindow : EditorWindow
    {
        // Pretend this mirrors your real FlowchartWindow API
        public object Flowchart => _fakeFlowchart;  // adapt type if your manager expects a concrete type
        public VisualElement Root => rootVisualElement;

        private FakeFlowchart _fakeFlowchart;
        private FcWindowVariablesComponent _component;

        // Load your UXML from the same path you use in FlowchartWindow.OnEnable
        // Make sure the asset exists at this Resources path.
        private const string UxmlResourcesPath = AmanitaConstants.PathToAmanitaVariableDisplayEditorUxml;

        [MenuItem("Tools/Amanita/Tests/Variables Panel Harness")]
        public static void Open()
        {
            var w = GetWindow<VariablesPanelHarnessWindow>();
            w.titleContent = new GUIContent("Variables Panel Harness");
            w.minSize = new Vector2(520, 320);
            w.Show();
        }

        private void OnEnable()
        {
            // Build fake flowchart with some seed data
            _fakeFlowchart = new FakeFlowchart();
            _fakeFlowchart.AddVariable("hp");
            _fakeFlowchart.AddVariable("mp");

            // Add a small toolbar to trigger add/remove for quick smoke tests
            var toolbar = new Toolbar();
            var addBtn = new ToolbarButton(() => _fakeFlowchart.AddVariable()) { text = "Add Var" };
            var rmBtn = new ToolbarButton(() => _fakeFlowchart.RemoveLast()) { text = "Remove Last" };
            toolbar.Add(addBtn);
            toolbar.Add(rmBtn);
            Root.Add(toolbar);

            // Instantiate your component
            _component = new FcWindowVariablesComponent
            {
                VariableDisplayEditorUxml = Resources.Load<VisualTreeAsset>(UxmlResourcesPath)
            };

            // Adapt this shim to the component's expectations
            // We mimic FlowchartWindow enough for Initialize to succeed.
            var flowchartAdapter = new ShimFlowchartWindow(this);
            _component.Initialize(flowchartAdapter);
        }

        private void OnDisable()
        {
            _component?.Dispose();
            _component = null;
        }

        // This shim adapts VariablesPanelHarnessWindow to the FcWindowVariablesComponent’s expected host
        private class ShimFlowchartWindow : FlowchartWindow
        {
            private readonly VariablesPanelHarnessWindow _host;

            public ShimFlowchartWindow(VariablesPanelHarnessWindow host)
            {
                _host = host;
            }

            // Expose what FcWindowVariablesComponent queries
            public new VisualElement rootVisualElement => _host.Root;

            // Adapt this to the exact type your VariableRowManager expects.
            // If it expects a concrete Flowchart type, you can replace FakeFlowchart with one,
            // or wrap FakeFlowchart in an adapter that exposes the same API.
            public new object Flowchart => _host.Flowchart;

        }
    }
}