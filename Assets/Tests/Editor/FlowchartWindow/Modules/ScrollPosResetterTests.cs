using System;
using System.Collections.Generic;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using UnityEngine;
using UnityObj = UnityEngine.Object;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using FcWindow = AtMycelia.Hyphlow.EditorUtils.FcWindow.FlowchartWindow;
namespace VScriptingTests.FlowchartWindow.Modules
{
    public sealed class ScrollPosResetterTests
    {
        private readonly IList<UnityObj> toDestroy = new List<UnityObj>();
        private FlowchartContext context;
        private FcWindow windowStub;
        private ScrollPosResetter resetter;
        private Flowchart flowchart;

        [SetUp]
        public void SetUp()
        {
            GameObject flowchartGo = new GameObject("Flowchart_Resetter_Test");
            flowchart = flowchartGo.AddComponent<Flowchart>();
            flowchart.ScrollPos = new Vector2(100f, -42f);

            context = new FlowchartContext
            {
                Flowchart = flowchart
            };

            windowStub = ScriptableObject.CreateInstance<TestFlowchartWindow>();
            resetter = new ScrollPosResetter(context);
            resetter.Initialize(windowStub);

            toDestroy.Add(flowchartGo);
            toDestroy.Add(windowStub);
        }

        [TearDown]
        public void TearDown()
        {
            resetter?.Dispose();
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
        public void ShiftPlusRKeyDown_ResetsScroll_AndBroadcastsWindowPan()
        {
            bool panRaised = false;
            Action handler = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += handler;

            try
            {
                resetter.OnGUI(KeyDown(KeyCode.R, shift: true));

                Assert.That(flowchart.ScrollPos, Is.EqualTo(Vector2.zero));
                Assert.That(panRaised, Is.True);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= handler;
            }
        }

        [Test]
        public void NonMatchingEvent_DoesNothing()
        {
            Vector2 originalScroll = flowchart.ScrollPos;
            bool panRaised = false;
            Action handler = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += handler;

            try
            {
                resetter.OnGUI(KeyDown(KeyCode.T, shift: true));

                Assert.That(flowchart.ScrollPos, Is.EqualTo(originalScroll));
                Assert.That(panRaised, Is.False);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= handler;
            }
        }

        [Test]
        public void ShiftPlusR_WithNullFlowchart_DoesNotRaisePan()
        {
            context.Flowchart = null;
            bool panRaised = false;
            Action handler = () => panRaised = true;
            FlowchartWindowSignals.WindowPanned += handler;

            try
            {
                resetter.OnGUI(KeyDown(KeyCode.R, shift: true));

                Assert.That(panRaised, Is.False);
            }
            finally
            {
                FlowchartWindowSignals.WindowPanned -= handler;
            }
        }

        private static Event KeyDown(KeyCode key, bool shift = false) =>
            new Event
            {
                type = EventType.KeyDown,
                keyCode = key,
                shift = shift
            };

        private sealed class TestFlowchartWindow : FcWindow
        {
            private new void OnEnable() { }
            private new void OnDisable() { }
            private new void OnDestroy() { }
        }
    }
}