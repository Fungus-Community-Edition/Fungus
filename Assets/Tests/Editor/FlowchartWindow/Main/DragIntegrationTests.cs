using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow;
using UnityObj = UnityEngine.Object;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using FcWindow = AtMycelia.Hyphlow.EditorUtils.FcWindow.FlowchartWindow;

namespace VScriptingTests.FCWindowOperations.Integration
{
    public class DragIntegrationTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            Selection.activeGameObject = flowchart.gameObject;
            EditorSelectionTracker.ResolveActiveFlowchart();

            window = ScriptableObject.CreateInstance<FcWindow>();
            SetWindowContext(window, ctx);

            hitDetector = new HitDetector();
            selectionBoxTracker = new SelectionBoxDragTrackerUitk(ctx);
            blockDragHandler = new BlockDragHandler(ctx);
            singleClickSelector = new SingleSelectionHandler(ctx);

            hitDetector.Initialize(window);
            selectionBoxTracker.Initialize(window);
            blockDragHandler.Initialize(window);
            singleClickSelector.Initialize(window);

            mouseButtonReleased = new Event { type = EventType.MouseUp, button = MouseButton.Left };
            mouseDrag = new Event { type = EventType.MouseDrag, button = MouseButton.Left, delta = dragDelta };
        }

        [TearDown]
        public override void TearDown()
        {
            hitDetector?.Dispose();
            selectionBoxTracker?.Dispose();
            blockDragHandler?.Dispose();
            singleClickSelector?.Dispose();

            if (window != null)
            {
                UnityObj.DestroyImmediate(window);
                window = null;
            }

            Selection.activeGameObject = null;

            base.TearDown();
        }

        protected Event mouseButtonReleased;
        protected Vector2 dragDelta = new Vector2(5, 7);

        private HitDetector hitDetector;
        private SelectionBoxDragTrackerUitk selectionBoxTracker;
        private BlockDragHandler blockDragHandler;
        private SingleSelectionHandler singleClickSelector;
        private FcWindow window;

        private static void SetWindowContext(FcWindow targetWindow, FlowchartContext context)
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            FieldInfo field = typeof(FcWindow).GetField("_fcContext", flags);
            field.SetValue(targetWindow, context);
        }

        private static PointerEventInfo CreatePointerInfo(Vector2 position, Vector2 delta)
        {
            return new PointerEventInfo(position, position, delta, delta);
        }

        private void HandleLeftMouseDown(Vector2 position, Event evt)
        {
            PointerEventInfo info = CreatePointerInfo(position, Vector2.zero);
            hitDetector.OnLeftMouseDown(info);

            if (ctx.Interaction.BlockHitInLastMouseDown == null)
            {
                selectionBoxTracker.OnEmptySpaceLeftMouseDown(info, evt);
            }
        }

        private void HandleLeftMouseDrag(Vector2 startPosition, Vector2 currentPosition, 
            Vector2 delta, Event evt)
        {
            PointerEventInfo dragStartInfo = CreatePointerInfo(startPosition, Vector2.zero);
            selectionBoxTracker.OnLeftMouseDragStarted(dragStartInfo, evt);
            blockDragHandler.OnLeftMouseDragStarted(dragStartInfo, evt);

            PointerEventInfo dragInfo = CreatePointerInfo(currentPosition, delta);
            selectionBoxTracker.OnLeftMouseDragged(dragInfo, evt);
            blockDragHandler.OnLeftMouseDragged(dragInfo, evt);
        }

        private void HandleLeftMouseDragEnd(Vector2 position, Event evt)
        {
            PointerEventInfo info = CreatePointerInfo(position, Vector2.zero);
            selectionBoxTracker.OnLeftMouseDragEnded(info, evt);
        }

        private void HandleLeftMouseUp(Vector2 position, Event evt)
        {
            PointerEventInfo info = CreatePointerInfo(position, Vector2.zero);
            blockDragHandler.OnLeftMouseUp(info, evt);

            if (ctx.Interaction.BlockHitInLastMouseDown == null)
            {
                selectionBoxTracker.OnEmptySpaceLeftMouseUp(info, evt);
            }
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void Drag_SelectedBlock_MovesByDelta(int blockIndex)
        {
            // 1) MouseDown on block #1 to select + start drag
            Vector2 baseBlockPos = initBlockPositions[blockIndex];
            Block targetBlock = blocks[blockIndex];
            mouseDown.mousePosition = initBlockPositions[blockIndex];

            HandleLeftMouseDown(baseBlockPos, mouseDown);
            singleClickSelector.OnBlockClicked(targetBlock, mouseDown);

            // block should now be selected and drag able to begin
            var selection = ctx.Selection;
            bool blockWasSelected = selection.Blocks.Contains(targetBlock);
            Assert.IsTrue(blockWasSelected, "Intended block was not selected");
            var interaction = ctx.Interaction;
            Assert.AreEqual(targetBlock, interaction.BlockHitInLastMouseDown,
                "Intended block wasn't the last one hit in mouse down");

            // 2) MouseDrag moves the block
            HandleLeftMouseDrag(baseBlockPos, baseBlockPos, dragDelta, mouseDrag);

            // Expected movement = delta / zoom (zoom=1)
            Vector2 expected = baseBlockPos + dragDelta;
            Assert.AreEqual(expected, targetBlock._NodeRect.position,
                "Block did not move by the correct delta");
            
            // 3) MouseUp finalizes & clears drag
            mouseButtonReleased.mousePosition = expected;
            HandleLeftMouseDragEnd(expected, mouseButtonReleased);
            HandleLeftMouseUp(expected, mouseButtonReleased);

            // After up, no BlockDragOngoing and DragBlock == null
            Assert.IsFalse(interaction.BlockDragOngoing, "DragOngoing should be cleared");
            Assert.IsNull(interaction.RootBlockToDrag, "Drag block should be cleared");
        }

        static IEnumerable<int> BlockIndices()
        {
            return Enumerable.Range(0, initBlockPositions.Count);
        }

        [Test]
        public void Drag_UnselectedBlock_DoesNothing()
        {
            // 1) MouseDown on block #0 but do NOT select it first
            Block targetBlock = blocks[0];
            Vector2 initBlockPos = initBlockPositions[0];
            mouseDown.mousePosition = initBlockPos;

            HandleLeftMouseDown(initBlockPos, mouseDown);
            singleClickSelector.OnBlockClicked(targetBlock, mouseDown);

            // Deselect for this test
            flowchart.ClearSelectedBlocks();
            var interaction = ctx.Interaction;
            interaction.BlockHitInLastMouseDown = targetBlock;
            interaction.RootBlockToDrag = null;

            // Now mouseDrag: no block selected so no drag
            HandleLeftMouseDrag(initBlockPos, initBlockPos, dragDelta, mouseDrag);
            HandleLeftMouseUp(initBlockPos + dragDelta, mouseButtonReleased);

            // block stays in place
            bool blockStayedInPlace = initBlockPos.Equals(targetBlock._NodeRect.position);
            Assert.IsTrue(blockStayedInPlace, "Block did not stay in place");
        }

        [Test]
        public void DragOutsideEmpty_DoesNotStartBoxOrDrag()
        {
            Vector2 start = new Vector2(150, 150);
            Vector2 end = new Vector2(160, 160);
            Vector2 delta = end - start;

            // click in empty space
            mouseDown.mousePosition = start;
            HandleLeftMouseDown(start, mouseDown);

            // drag in empty space: should continue marquee
            mouseDrag.mousePosition = end;
            HandleLeftMouseDrag(start, end, delta, mouseDrag);

            Assert.IsTrue(ctx.Interaction.SelectionBoxDragOngoing, "Box selection should be ongoing during drag");

            // mouse up: finalize marquee (select none)
            mouseButtonReleased.mousePosition = end;
            HandleLeftMouseDragEnd(end, mouseButtonReleased);
            HandleLeftMouseUp(end, mouseButtonReleased);

            Assert.IsFalse(ctx.Interaction.SelectionBoxDragOngoing, "Box selection drag state should be cleared");
            Assert.IsEmpty(flowchart.SelectedBlocks);
        }
    }
}