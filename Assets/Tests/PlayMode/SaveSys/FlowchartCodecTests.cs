using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TestTools;
using Amanita.VScripting;

namespace SaveSystemTests
{
    public class FlowchartCodecTests : CommonTestFunctionality
    {
        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsUniqueId()
        {
            Assert.AreEqual(flowchart.UniqueId, flowchartSaveData.UniqueId);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsFlowchartName()
        {
            Assert.AreEqual(flowchart.name, flowchartSaveData.FlowchartName);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsSavedVars()
        {
            foreach (Variable var in flowchart.Variables)
            {
                IVarCodec forThisVar = CodecRegistry.GetCodec(var);
                if (forThisVar == null)
                {
                    continue;
                }

                bool hasVarWithTheId = flowchartSaveData.SavedVars.Any(v => v.ItemID == var.ItemID);
                Assert.IsTrue(hasVarWithTheId);
            }
        }

        [UnityTest]
        public virtual IEnumerator FlowchartSaveData_Constructor_SetsSavedBlocks()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            IList<Block> blocksToSave = (from elem in flowchart.GetExecutingBlocks()
                                                  where elem.IncludeInSaves
                                                  select elem).ToList();

            bool triedSavingTheAppropriateAmountOfBlocks = blocksToSave.Count == flowchartSaveData.SavedBlocks.Count;
            Assert.IsTrue(triedSavingTheAppropriateAmountOfBlocks);

            foreach (Block block in blocksToSave)
            {
                BlockSaveData blockSaveData = blockSaveCodec.EncodeToSave(block);
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