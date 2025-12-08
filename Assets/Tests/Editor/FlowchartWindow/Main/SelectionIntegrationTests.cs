using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Amanita.VScripting;

namespace VScriptingTests.FCWindowOperations.Integration
{
    public class SelectionIntegrationTests : FlowchartWindowTestsCommon
    {
        /// <summary>
        /// Simulates the “pre-pass” hit test that FlowchartWindow.OnGUI does
        /// by setting BlockHitInLastMouseDown on the context.
        /// </summary>
        void PrePassHitTest(Event e)
        {
            if (e.type == EventType.MouseDown)
            {
                ctx.BlockHitInLastMouseDown = ctx.TopmostBlockOverlapping(e.mousePosition);
            }
            // clear any old marquee state
            if (e.type == EventType.MouseDown)
                ctx.SelectionBox = Rect.zero;
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void ClickOnBlock_SelectsThatBlock(int blockIndex)
        {
            mouseDown.mousePosition = initBlockPositions[blockIndex];
            PrePassHitTest(mouseDown);

            bool consumed = pipeline.Process(mouseDown, ctx);
            string errorMessage = "The SingleSelectionHandler should never consume. When seeing a " +
                "hit, neither should BoxSelectionHandler";
            Assert.IsFalse(consumed, errorMessage);

            // Expect exactly that block to be selected
            Block blockWeExpect = blocks[blockIndex];
            errorMessage = "Click on a single block did not make it so only that one is selected";
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
            PrePassHitTest(mouseDown);

            bool consumed = pipeline.Process(mouseDown, ctx);
            Assume.That(consumed, "BoxSelectionHandler should've consumed the mouse down on empty space");

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse down on empty space should've cleared all blocks");
        }

        protected readonly Vector2 emptySpace = new Vector2(3, 3);

        [Test]
        public virtual void MouseDown_EmptySpace_NoBlocksSelected_NothingStillSelected()
        {
            mouseDown.mousePosition = emptySpace;
            PrePassHitTest(mouseDown);

            bool consumed = pipeline.Process(mouseDown, ctx);
            Assume.That(consumed, "BoxSelectionHandler should've consumed the mouse down on empty space");

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse down on empty space with no blocks selected should've left the selection empty");
        }

        [Test]
        public virtual void MouseUp_Empty_NothingSelected_RemainsCleared()
        {
            mouseReleased.mousePosition = emptySpace;
            PrePassHitTest(mouseReleased);

            bool consumed = pipeline.Process(mouseDown, ctx);
            Assume.That(consumed, "BoxSelectionHandler should've consumed the mouse down on empty space");

            bool success = flowchart.SelectedBlockCount == 0;
            Assert.IsTrue(success, "Mouse release on empty space with no blocks selected should've left the selection empty");
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_Empty_OneBlockSelectedByMarquee_SelectionStays(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            Vector2 blockPos = toSelect._NodeRect.position;
            Vector2 offset = BoxSelectionHandler.MinThreshold * 2;

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

            bool success = ctx.SelectedBlockCount == 1 && flowchart.SelectedBlock == toSelect;
            string errorMessage = "Selecting a non-selected block should change the selection to only that block";
            Assert.IsTrue(success, errorMessage);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDown_OnAlreadySelected_SelectThatOneBlock(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            Vector2 blockPos = toSelect._NodeRect.position;
            bool consumed = false;
            string errorMessage = string.Empty;

            SimulateSingleBlockSelection(toSelect);

            consumed = pipeline.Process(mouseDown, ctx);
            errorMessage = "With the mouse being on a block, nothing should have consumed the mouse down";
            Assert.IsFalse(consumed, errorMessage);

            errorMessage = "After clicking on an already-selected block, only that block should've been selected";
            bool noClear = flowchart.SelectedBlockCount == 1 && flowchart.SelectedBlock == toSelect;
            Assert.IsTrue(noClear, errorMessage);
        }

        protected void SimulateSingleBlockSelection(Block toSelect, bool controlClick = false)
        {
            mouseDown.control = controlClick;
            Vector2 blockPos = toSelect._NodeRect.position;
            mouseDown.mousePosition = blockPos;
            bool consumed = pipeline.Process(mouseDown, ctx);
            string errorMessage = "Nothing should have consumed the mouse down, what with the mouse being on a block";
            Assume.That(!consumed, errorMessage);

            if (!controlClick) // Ctrl-clicking can add to the selection, so...
            {
                bool blockSelected = flowchart.SelectedBlockCount == 1 && flowchart.SelectedBlock == toSelect;
                errorMessage = "Only the one block should've been selected in the prep";
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
            mouseDown.mousePosition = emptySpace;
            PrePassHitTest(mouseDown);

            mouseDown.mousePosition = startMousePos;
            bool consumed = pipeline.Process(mouseDown, ctx);
            string errorMessage = "BoxSelectionHandler should've consumed the mouse up";
            Assume.That(consumed, errorMessage);

            mouseDrag.mousePosition = endMousePos;
            mouseDrag.delta = endMousePos - startMousePos;
            pipeline.Process(mouseDrag, ctx);
            errorMessage = "BoxSelectionHandler should've consumed the mouse drag";
            Assume.That(consumed, errorMessage);

            mouseReleased.mousePosition = mouseDrag.mousePosition;
            errorMessage = "BoxSelectionHandler should've consumed the mouse release";
            pipeline.Process(mouseReleased, ctx);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_ResetsSelectionBox(int blockIndex)
        {
            Block toSelect = blocks[blockIndex];
            SimulateSingleBlockSelection(toSelect);
            bool success = ctx.SelectionBox.size == Vector2.zero;
            string errorMessage = "After selecting a block, mouse up should've reset the selection box";
            Assert.IsTrue(success, errorMessage);
        }

        [Test]
        public void CtrlClick_OnSelected_AddsToSelection()
        {
            Block firstBlock = blocks[0];
            SimulateSingleBlockSelection(firstBlock);

            Block secondBlock = blocks[1];
            SimulateSingleBlockSelection(secondBlock, true);

            bool justTwoBlocksSelected = flowchart.SelectedBlockCount == 2;
            bool theTwoWeExpectAreSelected = justTwoBlocksSelected && flowchart.SelectedBlocks.Contains(firstBlock) 
                && flowchart.SelectedBlocks.Contains(secondBlock);
            Assert.IsTrue(theTwoWeExpectAreSelected, "Only the first 2 blocks should be selected");
        }

        [Test]
        public void CtrlClick_OnSelected_RemovesFromSelection()
        {
            // pre-select blocks[0] and blocks[1]
            Block firstBlock = blocks[0];
            Block secondBlock = blocks[1];

            SimulateSingleBlockSelection(firstBlock);
            SimulateSingleBlockSelection(secondBlock, true);

            bool bothBlocksSelected = flowchart.SelectedBlocks.Contains(firstBlock) &&
                flowchart.SelectedBlocks.Contains(secondBlock);
            Assert.IsTrue(bothBlocksSelected, "Both blocks should be selected in the prep");

            SimulateSingleBlockSelection(secondBlock, true);
            bool onlyFirstBlockSelectedNow = flowchart.SelectedBlocks.Contains(firstBlock) &&
                flowchart.SelectedBlockCount == 1;

            Assert.IsTrue(onlyFirstBlockSelectedNow, "Only the first Block should be selected after ctrl-clicking the second one");
        }

        [Test]
        public void CtrlClick_EmptySpace_DoesNotClear()
        {
            // pre-select block[2]
            SimulateSingleBlockSelection(blocks[2]);

            var e = new Event { type = EventType.MouseUp, button = 0, control = true };
            ctx.BlockHitInLastMouseDown = null;
            pipeline.Process(e, ctx);

            Assert.That(flowchart.SelectedBlocks, Is.EquivalentTo(new[] { blocks[2] }));
        }

    }
}