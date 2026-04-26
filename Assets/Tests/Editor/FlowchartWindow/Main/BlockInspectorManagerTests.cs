using System.Collections.Generic;
using AtMycelia.Hyphlow;
using AtMycelia.Hyphlow.EditorUtils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests.FlowchartWindow.Main
{
    public sealed class BlockInspectorManagerTests
    {
        private readonly IList<UnityObj> objectsToDestroy = new List<UnityObj>();
        private GameObject flowchartGo;
        private Flowchart flowchart;
        private Block block;

        [SetUp]
        public void SetUp()
        {
            ResetManagerState();

            flowchartGo = new GameObject("BlockInspectorManager_Flowchart");
            flowchart = flowchartGo.AddComponent<Flowchart>();
            block = flowchart.CreateBlock(new Vector2(10, 10));
            block.BlockName = "Test Block";

            objectsToDestroy.Add(flowchartGo);
        }

        private static void ResetManagerState()
        {
            Flowchart current = BlockInspectorManager.CurrentFlowchart;
            FlowchartWindowSignals.ChangedFlowchart(current, null);
            BlockInspectorManager.Clear();
            Selection.activeObject = null;
            Selection.activeGameObject = null;
        }

        [TearDown]
        public void TearDown()
        {
            ResetManagerState();

            for (int i = 0; i < objectsToDestroy.Count; i++)
            {
                UnityObj candidate = objectsToDestroy[i];
                if (candidate != null)
                {
                    UnityObj.DestroyImmediate(candidate);
                }
            }

            objectsToDestroy.Clear();
        }

        [Test]
        public void Show_AssignsInspectorAndUpdatesSelection()
        {
            BlockInspectorManager.Show(block);

            BlockInspector inspector = BlockInspectorManager.Inspector;

            Assert.That(BlockInspectorManager.LastShownBlock, Is.SameAs(block));
            Assert.That(BlockInspectorManager.CurrentFlowchart, Is.SameAs(flowchart));
            Assert.That(inspector._block, Is.SameAs(block));
            Assert.That(Selection.activeObject, Is.SameAs(inspector));
        }

        [Test]
        public void Clear_AfterShow_FocusesFlowchartAndClearsCommands()
        {
            flowchart.ClearSelectedCommands();
            TestCommand command = flowchart.AddCommand<TestCommand>(block);
            
            BlockInspectorManager.Show(block);
            flowchart.AddSelectedCommand(command); // Commands should only be selected when the right block is being shown

            Assert.That(flowchart.SelectedCommandCount, Is.EqualTo(1));

            Selection.activeObject = null;
            Selection.activeGameObject = null;

            BlockInspectorManager.Clear();

            Assert.That(flowchart.SelectedCommandCount, Is.EqualTo(0));
            Assert.That(BlockInspectorManager.LastShownBlock, Is.Null);
            Assert.That(Selection.activeGameObject, Is.SameAs(flowchart.gameObject));
        }

        [Test]
        public void BlockSelectedSignal_ShowsInspector()
        {
            flowchart.SelectedBlock = block;

            Assert.That(BlockInspectorManager.LastShownBlock, Is.SameAs(block));
            Assert.That(Selection.activeObject, Is.SameAs(BlockInspectorManager.Inspector));
        }

        [Test]
        public void BlockRemovedFromSelectionSignal_WithNoSelection_ClearsInspector()
        {
            flowchart.SelectedBlock = block;

            flowchart.ClearSelectedBlocks();
            Selection.activeObject = null;
            Selection.activeGameObject = null;

            BlockSignals.BlockDeselected(block);

            Assert.That(BlockInspectorManager.LastShownBlock, Is.Null);
            Assert.That(Selection.activeGameObject, Is.SameAs(flowchart.gameObject));
        }

        [Test]
        public void EmptySpaceClicked_ClearsInspectorAndFocusesFlowchart()
        {
            BlockInspectorManager.Show(block);
            Selection.activeObject = null;
            Selection.activeGameObject = null;

            PointerEventInfo newInfo = new PointerEventInfo(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            FlowchartWindowSignals.EmptySpaceLeftClicked(newInfo);

            Assert.That(BlockInspectorManager.LastShownBlock, Is.Null);
            Assert.That(Selection.activeGameObject, Is.SameAs(flowchart.gameObject));
        }

        [Test]
        public void ChangedFlowchart_WithNullNext_ClearsTrackedFlowchart()
        {
            BlockInspectorManager.Show(block);

            FlowchartWindowSignals.ChangedFlowchart(flowchart, null);

            Assert.That(BlockInspectorManager.CurrentFlowchart, Is.Null);
            Assert.That(BlockInspectorManager.LastShownBlock, Is.Null);
        }

        [Test]
        public void ChangedFlowchart_WithNoSelection_FocusesNewFlowchart()
        {
            GameObject otherGo = new GameObject("BlockInspectorManager_OtherFlowchart");
            Flowchart otherFlowchart = otherGo.AddComponent<Flowchart>();
            objectsToDestroy.Add(otherGo);

            Selection.activeGameObject = null;

            FlowchartWindowSignals.ChangedFlowchart(flowchart, otherFlowchart);

            Assert.That(BlockInspectorManager.CurrentFlowchart, Is.SameAs(otherFlowchart));
            Assert.That(Selection.activeGameObject, Is.SameAs(otherFlowchart.gameObject));
        }

        [Test]
        public void InspectorTargetChanged_FiresOnShowAndClear()
        {
            IList<Block> received = new List<Block>();
            void Handler(Block target) => received.Add(target);

            BlockInspectorManager.InspectorTargetChanged += Handler;

            try
            {
                BlockInspectorManager.Show(block);
                BlockInspectorManager.Clear();

                Assert.That(received.Count, Is.EqualTo(2));
                Assert.That(received[0], Is.SameAs(block));
                Assert.That(received[1], Is.Null);
            }
            finally
            {
                BlockInspectorManager.InspectorTargetChanged -= Handler;
            }
        }

        

        private sealed class TestCommand : Command
        {
        }
    }
}