using Amanita.EditorUtils;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using Amanita;

namespace VScriptingTests.FCWindowOperations
{
    public class BlockDragHandlerTests
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
                blocksInFlowchart = flowchart.CreateMultiBlocks(initBlockPositions);

                SizeAndPositionBlocks();
                void SizeAndPositionBlocks()
                {
                    for (int i = 0; i < blocksInFlowchart.Count; i++)
                    {
                        Vector2 initPos = initBlockPositions[i];
                        Block block = blocksInFlowchart[i];
                        Rect blockRect = block._NodeRect;
                        blockRect.position = initPos;
                        blockRect.size = blockSize;
                        block._NodeRect = blockRect;
                    }
                }
            }

            handler = new BlockDragHandler();

            fcContext = new FlowchartContext()
            {
                Flowchart = flowchart,
                Position = initPosition,
                SelectionBox = noSelectionBox,
            };

            PrepEvents();
            void PrepEvents()
            {
                mouseDownEvent = new Event()
                {
                    type = EventType.MouseDown,
                    mousePosition = initBlockPositions[0],
                };

                mouseDragEvent = new Event()
                {
                    type = EventType.MouseDrag,
                    mousePosition = initBlockPositions[0],
                    delta = dragDelta
                };

                mouseUpEvent = new Event()
                {
                    type = EventType.MouseUp,
                    mousePosition = initBlockPositions[0]
                };

                // ^The mouse positions are set to one of the block positions so that each
                // test starts with the mouse over an unselected block. As a result, 
                // we reduce boilerplate.

            }

            SetGridSnap(initGridSnap);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("");
            

        }

        protected GameObject fcHolder;
        protected Flowchart flowchart;
        protected readonly Vector2 initScrollPos = Vector2.zero;
        protected IList<Block> blocksInFlowchart;
        protected static readonly IList<Vector2> initBlockPositions = new List<Vector2>()
        {
            new Vector2(0.14f, 0.14f),
            new Vector2(5.345f, 5.345f),
            new Vector2(10.12f, 10.12f)
            // ^We have them as non-whole nums to test snapping
        };
        protected readonly Vector2 blockSize = new Vector2(100, 30);

        protected BlockDragHandler handler;
        protected FlowchartContext fcContext;
        protected readonly Rect initPosition = new Rect(0, 0, 500, 500);
        protected readonly Rect noSelectionBox = default;

        protected Event mouseDownEvent, mouseDragEvent,
            mouseUpEvent;
        protected Vector2 mousePos = new Vector2(100, 100);
        protected readonly Vector2 dragDelta = new Vector2(5, 10);

        protected virtual void SetGridSnap(bool val)
        {
            AmanitaEditorPreferences.useGridSnap = val;
        }

        protected readonly bool initGridSnap = false;

        [TearDown]
        public virtual void TearDown()
        {
            UnityObject.DestroyImmediate(fcHolder);
            blocksInFlowchart = null;
            fcContext = null;
            handler = null;

            ResetEvents();
            void ResetEvents()
            {
                mouseDownEvent = mouseDragEvent = mouseUpEvent = null;
            }

            SetGridSnap(false);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("");
        }

        [Test]
        public virtual void MouseDown_UnselectedBlock_NoConsume()
        {
            bool consumed = handler.Handle(mouseDownEvent, fcContext);
            Assert.IsFalse(consumed, "Consumed a mouse down event with no selected blocks.");
        }

        [Test]
        public virtual void MouseDown_UnselectedBlock_NoDragBlockSet()
        {
            handler.Handle(mouseDownEvent, fcContext);
            bool success = fcContext.RootBlockToDrag == null;
            Assert.IsTrue(success, "Drag Block was set after MouseDown on unselected Block");
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDown_SelectedBlock_YesConsume(int blockIndex)
        {
            SelectBlock(blockIndex);
            //mouseDownEvent.mousePosition = MousePositionFor(blockIndex);
            Block blockHit = blocksInFlowchart[blockIndex];
            fcContext.BlockHitInLastMouseDown = blockHit;
            bool consumed = handler.Handle(mouseDownEvent, fcContext);
            Assert.IsTrue(consumed, $"Block #{blockIndex} should consume MouseDown");
        }

        static IEnumerable<int> BlockIndices()
        {
            return Enumerable.Range(0, initBlockPositions.Count);
        }

        protected void SelectBlock(int blockIndex)
        {
            var toSelect = blocksInFlowchart[blockIndex];
            flowchart.AddToSelection(toSelect);
        }

        protected Vector2 MousePositionFor(int blockIndex)
        {
            return initBlockPositions[blockIndex];
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDrag_FirstMovement_RecordsUndoGroup(int blockIndex)
        {
            // Arrange
            SelectBlock(blockIndex);
            mouseDownEvent.mousePosition = MousePositionFor(blockIndex);
            fcContext.RootBlockToDrag = blocksInFlowchart[blockIndex];

            // Act
            bool consumed = handler.Handle(mouseDragEvent, fcContext);

            // Assert
            Assert.IsTrue(consumed, $"Block #{blockIndex} drag should be consumed");
            Assert.AreEqual(
            handler.startBlockDragGroupName,
            Undo.GetCurrentGroupName(),
            $"Block #{blockIndex} did not register undo on first drag"
                    );

            //string assertErrorMessage = "Should consume mouseDown on selected block";
            //Assert.IsTrue(consumed, assertErrorMessage);

            //string groupName = Undo.GetCurrentGroupName();
            //assertErrorMessage = $"Block #{blockIndex} did not register undo on first drag";
            //Assert.AreEqual(handler.startBlockDragGroupName, groupName, assertErrorMessage);

        }

        [Test]
        public virtual void MouseDrag_ValidDragBlock_MoveAllBlocksCorrectDist()
        {
            flowchart.AddRangeToSelection(blocksInFlowchart);
            Block firstBlock = blocksInFlowchart[0];
            fcContext.RootBlockToDrag = firstBlock;

            Vector2 expectedMovement = mouseDragEvent.delta / flowchart.Zoom;

            handler.Handle(mouseDragEvent, fcContext);

            IList<Vector2> blockPositionsAfter = blocksInFlowchart.Select((elem) => elem._NodeRect.position).ToList();

            for (int i = 0; i < blocksInFlowchart.Count; i++)
            {
                Block currentBlock = blocksInFlowchart[i];
                Vector2 prevPos = initBlockPositions[i];
                Vector2 actualPos = currentBlock._NodeRect.position;

                Vector2 expectedPos = prevPos + expectedMovement;
                string assertErrorMessage = $"Did not move {currentBlock.BlockName} to the right position." +
                    $"\nExpected: {expectedPos}" +
                    $"\nWhat we got: {actualPos}";
                Assert.AreEqual(expectedPos, actualPos, assertErrorMessage);
            }
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_ValidDragBlock_SnapsDragBlock(int blockIndex)
        {
            SetGridSnap(true);
            Block blockToDrag = blocksInFlowchart[blockIndex];
            Rect rectBefore = blockToDrag._NodeRect;
            SimulateDraggingBlockAtIndex(blockIndex);

            handler.Handle(mouseUpEvent, fcContext);

            // If things were properly snapped, then the SnapPosition func should
            // return a rect equal to the one it was called on
            Rect rectAfter = blockToDrag._NodeRect;
            string assertErrorMessage = $"Block #{blockIndex} wasn't even moved after dragging";
            Assert.AreNotEqual(rectBefore, rectAfter, assertErrorMessage);

            Rect snappedRectAfter = blockToDrag._NodeRect.SnapPosition(fcContext.GridObjectSnap);
            
            assertErrorMessage = $"The snapping for Block #{blockIndex} didn't work as intended.\n" + 
                $"Rect pos after drag: {rectAfter.position}\n" + 
                $"Expected rect pos after drag: {snappedRectAfter.position}";
            Assert.AreEqual(rectAfter, snappedRectAfter, assertErrorMessage);
            
        }

        protected virtual void SimulateDraggingBlockAtIndex(int blockIndex)
        {
            Block toDrag = blocksInFlowchart[blockIndex];
            mouseDragEvent.mousePosition = MousePositionFor(blockIndex);
            flowchart.AddToSelection(toDrag);
            fcContext.RootBlockToDrag = toDrag;
            handler.Handle(mouseDragEvent, fcContext);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_ValidDragBlock_ClearsDragBlock(int blockIndex)
        {
            SimulateDraggingBlockAtIndex(blockIndex);
            handler.Handle(mouseUpEvent, fcContext);
            string assertErrorMessage = $"Block #{blockIndex} was not cleared after being dragged and released";
            Assert.IsNull(fcContext.RootBlockToDrag, assertErrorMessage);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void MouseDown_SelectedBlock_UndoGroupNotRecorded(int blockIndex)
        {
            SelectBlock(blockIndex);
            Block blockHit = blocksInFlowchart[blockIndex];
            fcContext.BlockHitInLastMouseDown = blockHit;

            bool consumed = handler.Handle(mouseDownEvent, fcContext);

            Assert.IsTrue(consumed, $"Block #{blockIndex} should consume MouseDown");
            Assert.IsEmpty(Undo.GetCurrentGroupName(), $"Unexpected undo on MouseDown for block #{blockIndex}");
        }
    }
}