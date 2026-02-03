using System.Collections.Generic;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityObj = UnityEngine.Object;

namespace VScriptingTests.FlowchartWindow.Modules
{
    [TestFixture]
    public sealed class SingleClickBlockSelectorTests
    {
        private GameObject flowchartObject;
        private GameObject selectionObject;
        private Flowchart flowchart;
        private FlowchartContext context;
        private FlowchartWindowUitk window;
        private SingleClickBlockSelector syncer;
        private GameObject previousSelection;

        [SetUp]
        public void SetUp()
        {
            previousSelection = Selection.activeGameObject;

            flowchartObject = new GameObject("Flowchart_Test");
            flowchart = flowchartObject.AddComponent<Flowchart>();
            flowchart.UIModel.Owner = flowchartObject;

            selectionObject = new GameObject("Selection_Test");

            context = new FlowchartContext();
            context.Flowchart = flowchart;

            window = ScriptableObject.CreateInstance<FlowchartWindowUitk>();
            syncer = new SingleClickBlockSelector(context);
            syncer.Initialize(window);

            destroyOnTearDown.Add(flowchartObject);
            destroyOnTearDown.Add(selectionObject);
            destroyOnTearDown.Add(window);
        }

        private readonly IList<UnityObj> destroyOnTearDown = new List<UnityObj>();

        [TearDown]
        public void TearDown()
        {
            Selection.activeGameObject = previousSelection;

            foreach (UnityObj obj in destroyOnTearDown)
            {
                if (obj != null)
                {
                    UnityObj.DestroyImmediate(obj);
                }
            }
            destroyOnTearDown.Clear();
        }

        [Test]
        public void OnBlockClicked_SelectsBlockAndClearsCommands()
        {
            Block block = flowchart.CreateBlock(Vector2.zero);
            Command command = flowchart.AddCommand<DummyCommand>(block);

            flowchart.SelectedCommands = new List<Command> { command };
            syncer.OnBlockClicked(block, null);

            Assert.That(flowchart.SelectedBlock, Is.EqualTo(block));
            Assert.That(flowchart.SelectedCommandCount, Is.EqualTo(0));
        }

        [Test]
        public void OnBlockClicked_WhenBlockAlreadySelected_DoesNotClearCommands()
        {
            Block block = flowchart.CreateBlock(Vector2.zero);
            Command command = flowchartObject.AddComponent<DummyCommand>();

            flowchart.SelectedBlock = block;
            flowchart.SelectedCommands = new List<Command> { command };

            syncer.OnBlockClicked(block, null);

            Assert.That(flowchart.SelectedBlock, Is.EqualTo(block));
            Assert.That(flowchart.SelectedCommandCount, Is.EqualTo(1));
        }

        [Test]
        public void OnBlockClicked_NullClearsSelectionAndCommands()
        {
            Block block = flowchart.CreateBlock(Vector2.zero);
            Command command = flowchartObject.AddComponent<DummyCommand>();

            flowchart.SelectedBlock = block;
            flowchart.SelectedCommands = new List<Command> { command };

            syncer.OnBlockClicked(null, null);

            Assert.That(flowchart.SelectedBlock, Is.Null);
            Assert.That(flowchart.SelectedCommandCount, Is.EqualTo(0));
        }

        [Test]
        public void OnEmptySpaceClicked_ClearsSelectionAndSelectsFlowchartGameObject()
        {
            Block block = flowchart.CreateBlock(Vector2.zero);
            flowchart.SelectedBlock = block;
            Selection.activeGameObject = selectionObject;

            syncer.OnEmptySpaceClicked(Vector2.zero);

            Assert.That(flowchart.SelectedBlock, Is.Null);
            Assert.That(Selection.activeGameObject, Is.EqualTo(flowchartObject));
        }

        [Test]
        public void OnBlockCreated_SelectsBlock()
        {
            Block block = flowchart.CreateBlock(Vector2.zero);

            syncer.OnBlockCreated(block);

            Assert.That(flowchart.SelectedBlock, Is.EqualTo(block));
        }

        private sealed class DummyCommand : Command
        {
        }
    }
}