using Amanita.EditorUtils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;

namespace Amanita.Tests.Editor
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
                Position = initPosition
                
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
            flowchart.Zoom = initZoom; // The panning takes the zooming into account
        }

        [Test]
        public void IgnoresScroll_WhenNotInZoomMode()
        {
            PrepForPan(); // To make sure we're not in Zoom Mode
            Assert.Ignore();
        }

        [Test]
        public void DoesNotConsumeNonScrollEvents()
        {
            Assert.Ignore();
        }

        [Test]
        public void PansCanvas_OnRightMouseDrag()
        {
            Assert.Ignore();
        }

        [Test]
        public void PansCanvas_OnAltLeftMouseDrag()
        {
            Assert.Ignore();
        }

        [Test]
        public void DoesNotPan_WhenDragNotInPanModes()
        {
            Assert.Ignore();
        }
    }
}