using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;

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

            Selection.activeGameObject = fcHolder;
            EditorSelectionTracker.ResolveActiveFlowchart();

            fcContext = new FlowchartContext()
            {
                Flowchart = flowchart,
                Position = initPosition,
            };

            handler = new BlockDragHandler(fcContext);
            movementHandler = new BlockMovementHandler(fcContext);

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

            PrepPointerInfo();
            void PrepPointerInfo()
            {
                mouseDownInfo = PointerInfoFor(0, Vector2.zero);
                mouseDragInfo = PointerInfoFor(0, dragDelta);
                mouseUpInfo = PointerInfoFor(0, Vector2.zero);
            }

            SetGridSnap(initGridSnap);
            Undo.FlushUndoRecordObjects();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("");

            DecideWhatToDestroyDuringTearDown();
            void DecideWhatToDestroyDuringTearDown()
            {
                toDestroyInTearDown.Add(fcHolder);
            }
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
        protected BlockMovementHandler movementHandler;
        protected FlowchartContext fcContext;
        protected readonly Rect initPosition = new Rect(0, 0, 500, 500);
        protected readonly Rect noSelectionBox = default;

        protected Event mouseDownEvent, mouseDragEvent,
            mouseUpEvent;
        protected PointerEventInfo mouseDownInfo, mouseDragInfo, mouseUpInfo;
        protected Vector2 mousePos = new Vector2(100, 100);
        protected readonly Vector2 dragDelta = new Vector2(5, 10);

        private readonly IList<UnityObject> toDestroyInTearDown = new List<UnityObject>();

        protected virtual void SetGridSnap(bool val)
        {
            HyphlowEditorPreferences.useGridSnap = val;
        }

        protected readonly bool initGridSnap = false;

        [TearDown]
        public virtual void TearDown()
        {
            foreach (var obj in toDestroyInTearDown)
            {
                if (obj != null)
                {
                    UnityObject.DestroyImmediate(obj);
                }
            }
            toDestroyInTearDown.Clear();
            blocksInFlowchart = null;
            fcContext = null;
            handler = null;
            Selection.activeGameObject = null;

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
        public virtual void MouseDown_UnselectedBlock_RecordsHit()
        {
            handler.OnLeftMouseDown(mouseDownInfo);

            var interaction = fcContext.Interaction;
            Block expectedBlock = blocksInFlowchart[0];
            Assert.AreEqual(expectedBlock, interaction.BlockHitInLastMouseDown, 
                "Expected a hit on the block under the cursor.");
            Assert.IsNull(interaction.RootBlockToDrag, "Drag state should not start on mouse down.");
        }

        [Test]
        public virtual void MouseDown_UnselectedBlock_NoDragBlockSet()
        {
            handler.OnLeftMouseDown(mouseDownInfo);

            var interaction = fcContext.Interaction;
            Assert.IsNull(interaction.RootBlockToDrag, 
                "Drag Block was set after MouseDown on unselected Block");
            Assert.IsFalse(interaction.DragUndoRecorded, "Drag undo recorded on mouse down.");
            Assert.IsFalse(interaction.BlockDragOngoing, "Drag state started on mouse down.");
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDown_SelectedBlock_RecordsHit(int blockIndex)
        {
            SelectBlock(blockIndex);
            PointerEventInfo info = PointerInfoFor(blockIndex, Vector2.zero);

            handler.OnLeftMouseDown(info);

            Block blockHit = blocksInFlowchart[blockIndex];
            var interaction = fcContext.Interaction;
            Assert.AreEqual(blockHit, interaction.BlockHitInLastMouseDown, 
                $"Block #{blockIndex} should be registered as the hit block.");
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

        protected PointerEventInfo PointerInfoFor(int blockIndex, Vector2 delta)
        {
            Vector2 position = MousePositionFor(blockIndex);
            return new PointerEventInfo(position, position, delta, delta);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseDrag_FirstMovement_RecordsUndoGroup(int blockIndex)
        {
            // Arrange
            SelectBlock(blockIndex);
            var interaction = fcContext.Interaction;
            interaction.BlockHitInLastMouseDown = blocksInFlowchart[blockIndex];

            PointerEventInfo dragStartInfo = PointerInfoFor(blockIndex, Vector2.zero);
            PointerEventInfo dragInfo = PointerInfoFor(blockIndex, dragDelta);

            // Act
            handler.OnLeftMouseDragStarted(dragStartInfo, mouseDragEvent);
            handler.OnLeftMouseDragged(dragInfo, mouseDragEvent);

            // Assert
            Assert.IsTrue(interaction.DragUndoRecorded, 
                $"Block #{blockIndex} did not record undo on first drag");
            Assert.IsTrue(interaction.BlockDragOngoing, 
                $"Block #{blockIndex} did not register as dragging.");
        }

        [Test]
        public virtual void MouseDrag_ValidDragBlock_MoveAllBlocksCorrectDist()
        {
            flowchart.AddRangeToSelection(blocksInFlowchart);
            var interaction = fcContext.Interaction;
            interaction.BlockHitInLastMouseDown = blocksInFlowchart[0];

            PointerEventInfo dragStartInfo = PointerInfoFor(0, Vector2.zero);
            PointerEventInfo dragInfo = PointerInfoFor(0, dragDelta);

            handler.OnLeftMouseDragStarted(dragStartInfo, mouseDragEvent);
            handler.OnLeftMouseDragged(dragInfo, mouseDragEvent);

            Vector2 expectedMovement = dragDelta / flowchart.Zoom;

            for (int i = 0; i < blocksInFlowchart.Count; i++)
            {
                Block currentBlock = blocksInFlowchart[i];
                Vector2 prevPos = initBlockPositions[i];
                Vector2 actualPos = currentBlock._NodeRect.position;

                Vector2 expectedPos = prevPos + expectedMovement;
                string assertErrorMessage = $"Didn't move {currentBlock.BlockName} to the right position." +
                    $"\nExpected: {expectedPos}" +
                    $"\nWhat we got: {actualPos}";
                Assert.AreEqual(expectedPos, actualPos, assertErrorMessage);
            }
        }

        protected virtual void SimulateDraggingBlockAtIndex(int blockIndex)
        {
            Block toDrag = blocksInFlowchart[blockIndex];
            flowchart.AddToSelection(toDrag);

            var interaction = fcContext.Interaction;
            interaction.BlockHitInLastMouseDown = toDrag;

            PointerEventInfo dragStartInfo = PointerInfoFor(blockIndex, Vector2.zero);
            PointerEventInfo dragInfo = PointerInfoFor(blockIndex, dragDelta);

            handler.OnLeftMouseDragStarted(dragStartInfo, mouseDragEvent);
            handler.OnLeftMouseDragged(dragInfo, mouseDragEvent);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public virtual void MouseUp_ValidDragBlock_ClearsDragBlock(int blockIndex)
        {
            SimulateDraggingBlockAtIndex(blockIndex);
            handler.OnLeftMouseUp(mouseUpInfo, mouseUpEvent);

            string assertErrorMessage = $"Block #{blockIndex} was not cleared after " +
                $"being dragged and released";
            var interaction = fcContext.Interaction;
            Assert.IsNull(interaction.RootBlockToDrag, assertErrorMessage);
        }

        [Test, TestCaseSource(nameof(BlockIndices))]
        public void MouseDown_AlreadySelectedBlock_UndoGroupNotRecorded(int blockIndex)
        {
            SelectBlock(blockIndex);
            string undoGroupNameBefore = Undo.GetCurrentGroupName();
            PointerEventInfo info = PointerInfoFor(blockIndex, Vector2.zero);

            handler.OnLeftMouseDown(info);
            string undoGroupNameAfter = Undo.GetCurrentGroupName();
            Assert.AreEqual(undoGroupNameBefore, undoGroupNameAfter, 
                $"MouseDown on already selected block #{blockIndex} should not record an undo group.");
        }
    }
}