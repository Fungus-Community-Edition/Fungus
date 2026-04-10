using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;
using FcWindow = AtMycelia.Hyphlow.EditorUtils.FcWindow.FlowchartWindow;

namespace VScriptingTests.FCWindowOperations.Integration
{
    public class SelectionIntegrationTests : FlowchartWindowTestsCommon
    {
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            window = ScriptableObject.CreateInstance<FcWindow>();
            SetWindowContext(window, ctx);

            selectionBoxTracker = new SelectionBoxDragTrackerUitk(ctx);
            selectionBoxTracker.Initialize(window);

            singleSelectionHandler = new SingleSelectionHandler(ctx);
            singleSelectionHandler.Initialize(window);
        }

        [TearDown]
        public override void TearDown()
        {
            selectionBoxTracker?.Dispose();
            singleSelectionHandler?.Dispose();

            if (window != null)
            {
                ScriptableObject.DestroyImmediate(window);
                window = null;
            }

            base.TearDown();
        }

        private SelectionBoxDragTrackerUitk selectionBoxTracker;
        private SingleSelectionHandler singleSelectionHandler;
        private FcWindow window;

        private static void SetWindowContext(FcWindow targetWindow, FlowchartContext context)
        {
            FieldInfo field = typeof(FcWindow).GetField("_fcContext", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(targetWindow, context);
        }

        private static PointerEventInfo CreatePointerInfo(Vector2 position, Vector2 delta)
        {
            return new PointerEventInfo(position, position, delta, delta);
        }

        private void HandleMouseDown(Event e)
        {
            PrePassHitTest(e);

            if (ctx.Interaction.BlockHitInLastMouseDown == null)
            {
                PointerEventInfo info = CreatePointerInfo(e.mousePosition, Vector2.zero);
                selectionBoxTracker.OnEmptySpaceLeftMouseDown(info, e);
            }
        }

        private void HandleMouseDrag(Vector2 startPosition, Vector2 currentPosition, Event e)
        {
            PointerEventInfo dragStartInfo = CreatePointerInfo(startPosition, Vector2.zero);
            selectionBoxTracker.OnLeftMouseDragStarted(dragStartInfo, e);

            PointerEventInfo dragInfo = CreatePointerInfo(currentPosition, currentPosition - startPosition);
            selectionBoxTracker.OnLeftMouseDragged(dragInfo, e);
        }

        private void HandleMouseDragEnd(Vector2 position, Event e)
        {
            PointerEventInfo info = CreatePointerInfo(position, Vector2.zero);
            selectionBoxTracker.OnLeftMouseDragEnded(info, e);
        }

        private void HandleMouseUp(Event e, bool allowEmptySpaceClick = true)
        {
            Block blockHit = ctx.Interaction.BlockHitInLastMouseDown;
            if (blockHit != null)
            {
                singleSelectionHandler.OnBlockClicked(blockHit, e);
                return;
            }

            PointerEventInfo info = CreatePointerInfo(e.mousePosition, Vector2.zero);
            selectionBoxTracker.OnEmptySpaceLeftMouseUp(info, e);

            if (allowEmptySpaceClick)
            {
                singleSelectionHandler.OnEmptySpaceLeftClicked(info);
            }
        }

        /// <summary>
        /// Simulates the “pre-pass” hit test that FlowchartWindow.OnGUI does
        /// by setting BlockHitInLastMouseDown on the context.
        /// </summary>
        void PrePassHitTest(Event inputEv)
        {
            var document = ctx.Document;
            if (inputEv.type == EventType.MouseDown)
            {
                var interaction = ctx.Interaction;
                interaction.BlockHitInLastMouseDown = document.TopmostBlockOverlapping(inputEv.mousePosition);
            }
            // clear any old marquee state
            if (inputEv.type == EventType.MouseDown)
            {
                ctx.Interaction.SelectionBox = Rect.zero;
            }
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void ClickOnBlock_SelectsThatBlock(int blockIndex)
        {
            mouseDown.mousePosition = initBlockPositions[blockIndex];
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = initBlockPositions[blockIndex];
            HandleMouseUp(mouseReleased);

            // Expect exactly that block to be selected
            Block blockWeExpect = blocks[blockIndex];
            string errorMessage = "Click on a single block did not make it so only that one is selected";
            CollectionAssert.AreEqual(
                new[] { blockWeExpect },
                flowchart.SelectedBlocks,
                errorMessage
            );
        }

        static IEnumerable<int> BlockIndices()
        {
            return Enumerable.Range(0, initBlockPositions.Count);
        }


        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDown_EmptySpace_OneBlockSelected_Clears(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            flowchart.SelectedBlock = toSelect;

            mouseDown.mousePosition = emptySpace;
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = emptySpace;
            HandleMouseUp(mouseReleased);

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse down on empty space should've cleared all blocks");
        }

        protected readonly Vector2 emptySpace = new Vector2(3, 3);

        [Test]
        public virtual void MouseDown_EmptySpace_NoBlocksSelected_NothingStillSelected()
        {
            mouseDown.mousePosition = emptySpace;
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = emptySpace;
            HandleMouseUp(mouseReleased);

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse down on empty space with no blocks selected should've left the selection empty");
        }

        [Test]
        public virtual void MouseUp_Empty_NothingSelected_RemainsCleared()
        {
            mouseDown.mousePosition = emptySpace;
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = emptySpace;
            HandleMouseUp(mouseReleased);

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse release on empty space with no blocks selected should've left the selection empty");
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_Empty_OneBlockSelectedByMarquee_SelectionStays(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            Vector2 blockPos = toSelect._NodeRect.position;
            Vector2 offset = SelectionBoxDragTrackerUitk.MinThreshold * 2;

            // We need to set up the mouse positions so we don't accidentally select 
            // multiple blocks
            Vector2 startMousePos = blockPos - offset;
            Vector2 endMousePos = blockPos + offset;
            SimulateBoxSelection(startMousePos, endMousePos);

            bool success = flowchart.SelectedBlock == toSelect;
            Assert.IsTrue(success, "After the box select, only that one block should've stayed selected");
        }

        [Test]
        public virtual void MouseDown_OnNonSelected_SelectOnlyThat()
        {
            Block toSelect = blocks[0];
            SimulateSingleBlockSelection(toSelect);

            toSelect = blocks[1];
            SimulateSingleBlockSelection(toSelect);

            var selection = ctx.Selection;
            bool success = selection.BlockCount == 1 && flowchart.SelectedBlock == toSelect;
            string errorMessage = "Selecting a non-selected block should change the selection to only that block";
            Assert.IsTrue(success, errorMessage);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDown_OnAlreadySelected_SelectThatOneBlock(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];

            SimulateSingleBlockSelection(toSelect);

            mouseDown.mousePosition = toSelect._NodeRect.position;
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = toSelect._NodeRect.position;
            HandleMouseUp(mouseReleased);

            string errorMessage = "After clicking on an already-selected block, only that block should've been selected";
            bool noClear = flowchart.SelectedBlockCount == 1 && flowchart.SelectedBlock == toSelect;
            Assert.IsTrue(noClear, errorMessage);
        }

        protected void SimulateSingleBlockSelection(Block toSelect, bool controlClick = false)
        {
            mouseDown.control = controlClick;
            mouseReleased.control = controlClick;

            Vector2 blockPos = toSelect._NodeRect.position;
            mouseDown.mousePosition = blockPos;
            HandleMouseDown(mouseDown);

            mouseReleased.mousePosition = blockPos;
            HandleMouseUp(mouseReleased);

            mouseDown.control = false;
            mouseReleased.control = false;

            if (!controlClick) // Ctrl-clicking is ignored by SingleSelectionHandler
            {
                bool blockSelected = flowchart.SelectedBlockCount == 1 && flowchart.SelectedBlock == toSelect;
                string errorMessage = "Only the one block should've been selected in the prep";
                Assume.That(blockSelected, errorMessage);
            }
        }

        [Test, TestCaseSource(nameof(MultiSelectionCases))]
        public void MouseDrag_Marquee_SelectsExpectedBlocks(Vector2 startMousePos,
            Vector2 endMousePos,
            int[] expectedIndices)
        {
            // 1) Perform box selection
            SimulateBoxSelection(startMousePos, endMousePos);

            // 2) Map actual selected blocks to their indices
            var actualIndices = flowchart
                .SelectedBlocks
                .Select(b => blocks.IndexOf(b))
                .OrderBy(i => i)
                .ToArray();

            // 3) Assert equivalence
            Assert.That(
                actualIndices,
                Is.EquivalentTo(expectedIndices),
                $"Expected blocks [{string.Join(",", expectedIndices)}], " +
                $"but got [{string.Join(",", actualIndices)}]"
            );
        }

        static IEnumerable<TestCaseData> MultiSelectionCases()
        {
            // Drag from (0,0) to (60,60) → should pick up blocks[0] & blocks[1]
            yield return new TestCaseData(new Vector2(0, 0),
                new Vector2(60, 60),
                new[] { 0, 1 }
            ).SetName("Box_0_0_to_60_60_Selects_0_and_1");

            // Drag a giant marquee → selects all blocks
            yield return new TestCaseData(new Vector2(0, 0),
                new Vector2(200, 200),
                new[] { 0, 1, 2 }
            ).SetName("Box_0_0_to_200_200_Selects_All");

            // Drag around only the last block → selects blocks[2] alone
            yield return new TestCaseData(new Vector2(80, 80),
                new Vector2(120, 120),
                new[] { 2 }
            ).SetName("Box_80_80_to_120_120_Selects_2");

            // Drag in empty area → selects none
            yield return new TestCaseData(new Vector2(150, 150),
                new Vector2(180, 180),
                new int[0]
            ).SetName("Box_150_150_to_180_180_Selects_None");
        }

        protected virtual void SimulateBoxSelection(Vector2 startMousePos, Vector2 endMousePos)
        {
            mouseDown.mousePosition = startMousePos;
            HandleMouseDown(mouseDown);

            mouseDrag.mousePosition = endMousePos;
            mouseDrag.delta = endMousePos - startMousePos;
            HandleMouseDrag(startMousePos, endMousePos, mouseDrag);

            mouseReleased.mousePosition = mouseDrag.mousePosition;
            HandleMouseDragEnd(endMousePos, mouseReleased);
            HandleMouseUp(mouseReleased, false);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_ResetsSelectionBox(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            SimulateSingleBlockSelection(toSelect);
            bool success = ctx.Interaction.SelectionBox.size == Vector2.zero;
            string errorMessage = "After selecting a block, mouse up should've reset the selection box";
            Assert.IsTrue(success, errorMessage);
        }

        [Test]
        public void CtrlClick_OnSelected_DoesNotChangeSelection()
        {
            Block firstBlock = blocks[0];
            SimulateSingleBlockSelection(firstBlock);

            Block secondBlock = blocks[1];
            SimulateSingleBlockSelection(secondBlock, true);

            bool onlyFirstBlockSelected = flowchart.SelectedBlockCount == 1 &&
                flowchart.SelectedBlocks.Contains(firstBlock);
            Assert.IsTrue(onlyFirstBlockSelected, 
                "Ctrl-click should not change selection in SingleSelectionHandler.");
        }

        [Test]
        public void CtrlClick_OnSelected_DoesNotDeselect()
        {
            Block firstBlock = blocks[0];
            SimulateSingleBlockSelection(firstBlock);

            SimulateSingleBlockSelection(firstBlock, true);

            bool onlyFirstBlockSelected = flowchart.SelectedBlockCount == 1 &&
                flowchart.SelectedBlocks.Contains(firstBlock);
            Assert.IsTrue(onlyFirstBlockSelected, "Ctrl-click should not deselect in SingleSelectionHandler.");
        }

        [Test]
        public void CtrlClick_EmptySpace_ClearsSelection()
        {
            // pre-select block[2]
            SimulateSingleBlockSelection(blocks[2]);

            mouseDown.control = true;
            mouseDown.mousePosition = emptySpace;
            HandleMouseDown(mouseDown);

            mouseReleased.control = true;
            mouseReleased.mousePosition = emptySpace;
            HandleMouseUp(mouseReleased);

            Assert.IsEmpty(flowchart.SelectedBlocks);
        }

    }
}