using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests.FlowchartWindow.Modules
{
    public sealed class FlowchartSelectionSyncerUitkTests
    {
        [SetUp]
        public void SetUp()
        {
            flowchartGo = new GameObject("Flowchart_Test");
            flowchart = flowchartGo.AddComponent<Flowchart>();
            existingBlock = flowchartGo.AddComponent<Block>();

            context = new FlowchartContext
            {
                Flowchart = flowchart
            };

            stubWindow = ScriptableObject.CreateInstance<TestFlowchartWindow>();
            syncer = new FlowchartSelectionSyncerUitk(context);
            syncer.Initialize(stubWindow);

            Selection.activeGameObject = null;

            destroyInTearDown.Add(flowchartGo);
            destroyInTearDown.Add(stubWindow);
        }

        private GameObject flowchartGo;
        private Flowchart flowchart;
        private Block existingBlock;
        private FlowchartContext context;
        private FlowchartWindowUitk stubWindow;
        private FlowchartSelectionSyncerUitk syncer;
        private readonly IList<UnityObj> destroyInTearDown = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            syncer?.Dispose();
            context?.Dispose();
            foreach (var obj in destroyInTearDown)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            Selection.activeGameObject = null;
        }

        [Test]
        public void OnBlockSelected_SetsFlowchartSelection()
        {
            flowchart.ClearSelectedBlocks();
            syncer.OnBlockSelected(existingBlock);

            Assert.That(flowchart.SelectedBlock, Is.SameAs(existingBlock));
            Assert.That(flowchart.SelectedBlockCount, Is.EqualTo(1));
        }

        [Test]
        public void OnEmptySpaceClicked_ClearsSelection_AndFocusesFlowchart()
        {
            flowchart.SelectedBlock = existingBlock;
            Selection.activeGameObject = null;

            syncer.OnEmptySpaceClicked(Vector2.zero);

            Assert.That(flowchart.SelectedBlock, Is.Null);
            Assert.That(flowchart.SelectedBlockCount, Is.EqualTo(0));
            Assert.That(Selection.activeGameObject, Is.SameAs(flowchart.gameObject));
        }

        [Test]
        public void BlockCreatedSignal_SelectsNewBlock()
        {
            flowchart.ClearSelectedBlocks();
            Block newBlock = flowchartGo.AddComponent<Block>();

            BlockSignals.BlockCreated(newBlock);

            Assert.That(flowchart.SelectedBlock, Is.SameAs(newBlock));
        }

        private sealed class TestFlowchartWindow : FlowchartWindowUitk
        {
            // All no-ops for testing's sake
            protected override void OnEnable()
            {
            }

            protected override void OnDisable()
            {
            }

            protected override void OnDestroy()
            {
            }
        }
    }
}