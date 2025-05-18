using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;
using System.Linq;
using UnityEngine.TestTools;

namespace Amanita.SaveSystemTests
{
    public class FlowchartSaveDataTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsUniqueId()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            Assert.AreEqual(flowchart.UniqueId, flowchartSaveData.UniqueId);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsFlowchartName()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            Assert.AreEqual(flowchart.name, flowchartSaveData.FlowchartName);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsSavedVars()
        {
            FlowchartSaveData flowchartSaveData = new(flowchart);
            foreach (Variable var in flowchart.Variables)
            {
                IVarEncoder forThisVar = EncoderRegistry.GetEncoder(var);
                if (forThisVar == null)
                {
                    continue;
                }

                bool hasVarWithTheId = flowchartSaveData.SavedVars.Any(v => v.UniqueID == var.UniqueId);
                Assert.IsTrue(hasVarWithTheId);
            }
        }

        [UnityTest]
        public virtual IEnumerator FlowchartSaveData_Constructor_SetsSavedBlocks()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            FlowchartSaveData flowchartSaveData = new(flowchart);
            IList<Block> blocksToSave = (from elem in flowchart.GetExecutingBlocks()
                                                  where elem.SaveExecutionState
                                                  select elem).ToList();
            foreach (Block block in blocksToSave)
            {
                BlockSaveData blockSaveData = new(block);
                bool correctBlockName = blockSaveData.BlockName == block.BlockName;
                bool correctItemId = blockSaveData.ItemId == block.ItemId;
                bool correctActiveCommandId = blockSaveData.ActiveCommandId == block.ActiveCommand.ItemId;
                bool correctActiveCommandIndex = blockSaveData.ActiveCommandIndex == block.ActiveCommand.CommandIndex;
                Assert.IsTrue(correctBlockName, $"Block name mismatch for block {block.BlockName}.");
                Assert.IsTrue(correctItemId, $"Item ID mismatch for block {block.BlockName}.");
                Assert.IsTrue(correctActiveCommandId, $"Active command ID mismatch for block {block.BlockName}.");
                Assert.IsTrue(correctActiveCommandIndex, $"Active command index mismatch for block {block.BlockName}.");
            }
        }

    }
}