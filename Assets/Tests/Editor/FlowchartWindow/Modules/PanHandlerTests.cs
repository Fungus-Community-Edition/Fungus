using System;
using System.Collections.Generic;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using FcWindow = AtMycelia.Hyphlow.EditorUtils.FcWindow.FlowchartWindow;

namespace VScriptingTests.FcW.Modules
{
    public sealed class PanHandlerTests
    {
        private readonly IList<UnityObj> toDestroy = new List<UnityObj>();
        private FlowchartContext context;
        private FcWindow windowStub;
        private PanHandler handler;
        private Flowchart flowchart;

        [SetUp]
        public void SetUp()
        {
            GameObject go = new GameObject("Flowchart_PanHandler_Test");
            flowchart = go.AddComponent<Flowchart>();
            flowchart.ScrollPos = _initScrollPos;
            flowchart.Zoom = 2f;

            context = new FlowchartContext
            {
                Flowchart = flowchart
            };

            windowStub = ScriptableObject.CreateInstance<TestFlowchartWindow>();
            handler = new PanHandler(context);
            handler.Initialize(windowStub);

            toDestroy.Add(go);
            toDestroy.Add(windowStub);
        }

        private readonly Vector2 _initScrollPos = new Vector2(10f, 10f);

        [TearDown]
        public void TearDown()
        {
            handler?.Dispose();
            context?.Dispose();

            foreach (UnityObj obj in toDestroy)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }

            toDestroy.Clear();
        }

        [Test]
        public void ScrollWheelDrag_UpdatesScrollPos_AndRaisesWindowPanned()
        {
            bool panRaised = false;
            Action listener = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += listener;

            try
            {
                Vector2 direction = new Vector2(4f, -6f);
                handler.OnScrollWheelDragged(direction);

                Vector2 expectedDelta = direction / flowchart.Zoom;
                Vector2 expectedScroll = _initScrollPos - expectedDelta;

                Assert.That(flowchart.ScrollPos, Is.EqualTo(expectedScroll));
                Assert.That(panRaised, Is.True);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= listener;
            }
        }

        [Test]
        public void TinyDrag_IsIgnored()
        {
            flowchart.ScrollPos = new Vector2(1f, 1f);

            bool panRaised = false;
            Action listener = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += listener;

            try
            {
                handler.OnScrollWheelDragged(new Vector2(0.001f, 0.001f));

                Assert.That(flowchart.ScrollPos, Is.EqualTo(new Vector2(1f, 1f)));
                Assert.That(panRaised, Is.False);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= listener;
            }
        }

        [Test]
        public void NullFlowchart_NoOp()
        {
            context.Flowchart = null;

            bool panRaised = false;
            Action listener = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += listener;

            try
            {
                handler.OnScrollWheelDragged(new Vector2(5f, 5f));
                Assert.That(panRaised, Is.False);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= listener;
            }
        }

        private sealed class TestFlowchartWindow : FcWindow
        {
            private new void OnEnable() { }
            private new void OnDisable() { }
            private new void OnDestroy() { }
        }
    }
}