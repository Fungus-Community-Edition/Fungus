using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;

namespace VScriptingTests.FCWindowOperations
{
    public class PanZoomHandlerTests 
    {
        [SetUp]
        public virtual void SetUp()
        {
            PrepSceneObjects();
            void PrepSceneObjects()
            {
                fcHolder = new GameObject("Flowchart");
                flowchart = fcHolder.AddComponent<Flowchart>();
                flowchart.ScrollPos = initScrollPos;
            }

            handler = new PanZoomHandler();

            fcContext = new FlowchartContext()
            {
                Flowchart = flowchart,
                Position = initPosition,
                SelectionBox = noSelectionBox,
                
            };

            PrepEvents();
            void PrepEvents()
            {
                upwardsScrollEvent = new Event()
                {
                    type = EventType.ScrollWheel,
                    mousePosition = mousePos,
                    delta = upwardScroll
                };

                downwardsScrollEvent = new Event()
                {
                    type = EventType.ScrollWheel,
                    mousePosition = mousePos,
                    delta = downwardScroll
                };

                hugeUpwardsScrollEvent = new Event()
                {
                    type = EventType.ScrollWheel,
                    mousePosition = mousePos,
                    delta = hugeUpwardScroll
                };

                hugeDownwardsScrollEvent = new Event()
                {
                    type = EventType.ScrollWheel,
                    mousePosition = mousePos,
                    delta = hugeDownwardScroll
                };

                middleDragEvent = new Event()
                {
                    type = EventType.MouseDrag,
                    button = middleMouseButton,
                    mousePosition = mousePos
                };
            }
        }

        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected PanZoomHandler handler;
        protected readonly Vector2 initScrollPos = Vector2.zero;
        protected readonly Rect initPosition = new Rect(0, 0, 500, 500);

        protected FlowchartContext fcContext;
        
        protected Event upwardsScrollEvent, downwardsScrollEvent,
            hugeUpwardsScrollEvent, hugeDownwardsScrollEvent;
        protected Vector2 mousePos = new Vector2(100, 100);
        protected Event middleDragEvent;
        protected readonly int middleMouseButton = 2;
        protected readonly Rect noSelectionBox = default;

        [TearDown]
        public virtual void TearDown()
        {
            UnityObject.DestroyImmediate(fcHolder);
            fcContext = null;
            handler = null;

            ResetEvents();
            void ResetEvents()
            {
                upwardsScrollEvent = downwardsScrollEvent =
                    hugeUpwardsScrollEvent = hugeDownwardsScrollEvent = null;

                middleDragEvent = null;
            }

            RevertToolSettings();
        }

        protected virtual void RevertToolSettings()
        {
            Tools.current = Tool.None;
            Tools.viewTool = ViewTool.Pan;
        }

        [Test]
        public void ZoomsIn_OnScrollWheelMoveUp()
        {
            // Zoom should increase when scrolling up
            PrepForZoom();

            bool consumed = handler.Handle(upwardsScrollEvent, fcContext);

            Assert.IsTrue(consumed);
            Assert.Greater(flowchart.Zoom, initZoom);
        }

        protected virtual void PrepForZoom()
        {
            Tools.current = Tool.View;
            Tools.viewTool = ViewTool.Zoom;
            flowchart.Zoom = initZoom;
        }

        protected float initZoom = 0.5f;

        protected Vector2 upwardScroll = new Vector2(0, -10);

        [Test]
        public virtual void ZoomsOut_OnScrollWheelMoveDown()
        {
            // Zoom should fall when scrolling down
            PrepForZoom();
            bool consumed = handler.Handle(downwardsScrollEvent, fcContext);

            Assert.IsTrue(consumed);
            Assert.Less(flowchart.Zoom, initZoom);
        }

        protected Vector2 downwardScroll = new Vector2(0, 10);

        [Test]
        public virtual void ZoomOut_ClampsZoom_WithinRange()
        {
            PrepForZoom();
            bool consumed = handler.Handle(hugeDownwardsScrollEvent, fcContext);

            Assert.IsTrue(consumed);
            Assert.AreEqual(flowchart.Zoom, handler.MinZoom);
        }

        protected Vector2 hugeUpwardScroll = new Vector2(0, -10000),
            hugeDownwardScroll = new Vector2(0, 10000);

        [Test]
        public virtual void ZoomIn_ClampsZoom_WithinRange()
        {
            PrepForZoom();

            bool consumed = handler.Handle(hugeUpwardsScrollEvent, fcContext);

            Assert.IsTrue(consumed);
            Assert.AreEqual(flowchart.Zoom, handler.MaxZoom);
        }

        [Test]
        public void PansCanvas_OnMiddleMouseDrag()
        {
            PrepForPan();
            middleDragEvent.delta = new Vector2(10, -20);

            bool consumed = handler.Handle(middleDragEvent, fcContext);

            Assert.IsTrue(consumed, "Middle-drag should be consumed by PanZoomHandler");
            Vector2 expectedScrollPos = middleDragEvent.delta / initZoom;
            Assert.AreEqual(expectedScrollPos, flowchart.ScrollPos);
        }

        protected virtual void PrepForPan()
        {
            Tools.current = Tool.View;
            Tools.viewTool = ViewTool.Pan;
            flowchart.Zoom = initZoom;
            // ^The panning takes the zooming into account. The more zoomed out, the 
            // more you move the "world" (for lack of a better term)
        }

        [Test]
        public void IgnoresScroll_WhenNotInZoomMode()
        {
            PrepForPan(); // To make sure we're not in Zoom Mode

            bool consumed = handler.Handle(upwardsScrollEvent, fcContext);
            Assert.IsFalse(consumed, "Consumed an upwards scroll event when we should be in pan mode");

            consumed = handler.Handle(downwardsScrollEvent, fcContext);
            Assert.IsFalse(consumed, "Consumed a downwards scroll event when we should be in pan mode");

            consumed = handler.Handle(hugeUpwardsScrollEvent, fcContext);
            Assert.IsFalse(consumed, "Consumed a yuge upwards scroll event when we should be in pan mode");

            consumed = handler.Handle(hugeDownwardsScrollEvent, fcContext);
            Assert.IsFalse(consumed, "Consumed a yuge downwards scroll event when we should be in pan mode");
        }

        [Test]
        public void DoesNotConsumeNonScrollEvents()
        {
            var clickEvent = new Event { type = EventType.MouseDown, button = 0 };
            Assert.IsFalse(handler.Handle(clickEvent, fcContext));
        }

        [Test]
        public void PansCanvas_OnRightMouseDrag()
        {
            PrepForPan();
            var drag = new Event { type = EventType.MouseDrag, button = 1, delta = new Vector2(5, 5) };
            bool consumed = handler.Handle(drag, fcContext);

            Assert.IsTrue(consumed);
            Assert.AreEqual(new Vector2(5, 5) / initZoom, flowchart.ScrollPos);
        }

        [Test]
        public void PansCanvas_OnAltLeftMouseDrag()
        {
            PrepForPan();
            var drag = new Event { type = EventType.MouseDrag, button = 0, alt = true, delta = new Vector2(-7, 3) };
            bool consumed = handler.Handle(drag, fcContext);

            Assert.IsTrue(consumed);
            Assert.AreEqual(new Vector2(-7, 3) / initZoom, flowchart.ScrollPos);
        }

        [Test]
        public void DoesNotPan_WhenDragNotInPanModes()
        {
            // Left-drag without Alt, and using default Tool.None
            var drag = new Event { type = EventType.MouseDrag, button = 0, delta = new Vector2(8, -8) };
            flowchart.ScrollPos = Vector2.zero;

            bool consumed = handler.Handle(drag, fcContext);

            Assert.IsFalse(consumed);
            Assert.AreEqual(Vector2.zero, flowchart.ScrollPos);
        }

        [Test]
        public void IgnoresZoom_WhenSelectionBoxActive()
        {
            PrepForZoom();
            // simulate an active selection box
            fcContext.SelectionBox = new Rect(0, 0, 10, 10);

            bool consumed = handler.Handle(upwardsScrollEvent, fcContext);
            Assert.IsFalse(consumed);
        }

        [Test]
        public void IgnoresZoom_WhenInPanTool()
        {
            PrepForPan();
            //PrepForZoom();                  // puts Tools.viewTool == Zoom
            //Tools.viewTool = ViewTool.Pan;  // switch to Pan

            bool consumed = handler.Handle(upwardsScrollEvent, fcContext);

            Assert.IsFalse(consumed, "Should not zoom when the Pan tool is active");
        }

    }
}