using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Amanita.VScripting;
using Amanita.FSExt;

namespace SaveSystemTests
{
    public class BlockCodecTests : CommonTestFunctionality
    {
        protected override void PrepScene()
        {
            base.PrepScene();
            block = flowchart.FindBlock("TestBlock");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
        }

        protected Block block;
        protected BlockSaveData blockSaveData = null;

        [Test]
        public virtual void CorrectBlockID_ENcoded()
        {
            Assert.AreEqual(block.ItemId, blockSaveData.ItemId, "Block ID mismatch.");
        }

        [Test]
        public virtual void CorrectBlockName_ENcoded()
        {
            Assert.AreEqual(block.BlockName, blockSaveData.BlockName, "Block name mismatch.");
        }

        [Test]
        public virtual void CorrectBlockName_DEcoded()
        {
            string serializedStr = serializer.ToJson(blockSaveData);
            BlockSaveData deserializedBlock = serializer.FromJson<BlockSaveData>(serializedStr);
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }

        [Test]
        public virtual async Task CorrectBlockID_DEcoded()
        {
            await Task.Delay(100);
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            string serializedStr = serializer.ToJson(blockSaveData);
            BlockSaveData deserializedBlock = serializer.FromJson<BlockSaveData>(serializedStr);
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandID_ENcoded()
        {
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandID_DEcoded()
        {
            await Task.Delay(500);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            string serializedData = serializer.ToJson(blockSaveData);
            BlockSaveData deserializedBlock = serializer.FromJson<BlockSaveData>(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandIndex_ENcoded()
        {
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandIndex_DEcoded()
        {
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            string serializedData = serializer.ToJson(blockSaveData);
            BlockSaveData deserializedBlock = serializer.FromJson<BlockSaveData>(serializedData);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
        }

        [Test]
        public virtual async Task CorrectBlockSaveDataSerialized()
        {
            await Task.Delay(100);
            BlockSaveData beforeSerializing = blockSaveCodec.EncodeToSave(block);
            string serializedData = serializer.ToJson(beforeSerializing);
            BlockSaveData deserializedBlock = serializer.FromJson<BlockSaveData>(serializedData);

            Assert.IsNotNull(deserializedBlock, "Deserialized Block is null.");
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }


        [Test]
        public async Task EncodeToMultiSave_IncludeCorrectBlocks()
        {
            // The correct Blocks here being the ones that:
            // - have their Include In Saves flag set to true
            // - are executing at the time of saving
            await Task.Delay(100);
            IList<Block> allBlocks = flowchart.GetComponents<Block>();
            IList<Block> whatShouldNOTBeIncluded = (from elem in allBlocks
                                                    where !elem.IncludeInSaves || elem.IsExecuting()
                                                    select elem).ToList();
            Assume.That(whatShouldNOTBeIncluded.Count > 0, $"Test scene needs at least one Block in Flowchart {flowchart.name} that should NOT be included");

            IList<Block> whatShouldBeIncluded = (from elem in allBlocks
                                                 where elem.IsExecuting()
                                                 where elem.IncludeInSaves
                                                 select elem).ToList();
            Assume.That(whatShouldBeIncluded.Count > 0, $"Test scene needs at least one Block in Flowchart {flowchart.name} that SHOULD be included");

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(flowchart);
            Assert.IsTrue(result.Count == whatShouldBeIncluded.Count, "Encoded the wrong amount of Blocks");

            IList<ushort> idsThatShouldBeIncluded = whatShouldBeIncluded.Select(item => item.ItemId).ToList();
            IList<ushort> resultIDs = result.Select(item => item.ItemId).ToList();

            bool onlyTheRightStuff = idsThatShouldBeIncluded.SequenceEqual(resultIDs);

            Assert.IsTrue(onlyTheRightStuff, "Encoded at least one Block that shouldn't have been included");
        }

        [Test]
        public void EncodeToSave_RespectsFlowchartIncludeInSaves()
        {
            flowchart.IncludeInSaves = false;
            FlowchartSaveData saveData = flowchartSaveCodec.EncodeToSave(flowchart);
            Assert.IsNull(saveData); 
        }

        [Test]
        public async Task EncodeToMultiSave_ExcludesBlocksWithIncludeInSavesFalse()
        {
            await Task.Delay(100);
            IList<Block> allBlocks = flowchart.GetComponents<Block>();
            // Find all blocks that are executing but have IncludeInSaves == false
            var excludedBlocks = allBlocks.Where(b => !b.IncludeInSaves && b.IsExecuting()).ToList();
            Assume.That(excludedBlocks.Count > 0, "Test scene needs at least one executing Block with IncludeInSaves == false");

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(flowchart);
            var resultIDs = result.Select(b => b.ItemId).ToList();

            foreach (var block in excludedBlocks)
            {
                Assert.IsFalse(resultIDs.Contains(block.ItemId), $"Block {block.BlockName} (ID {block.ItemId}) should not be included when IncludeInSaves is false.");
            }
        }

        [Test]
        public async Task EncodeToMultiSave_ExcludesBlocksThatAreNotExecuting()
        {
            await Task.Delay(100);
            IList<Block> allBlocks = flowchart.GetComponents<Block>();
            // Find all blocks that have IncludeInSaves == true but are not executing
            var excludedBlocks = allBlocks.Where(b => b.IncludeInSaves && !b.IsExecuting()).ToList();
            Assume.That(excludedBlocks.Count > 0, "Test scene needs at least one non-executing Block with IncludeInSaves == true");

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(flowchart);
            var resultIDs = result.Select(b => b.ItemId).ToList();

            foreach (var block in excludedBlocks)
            {
                Assert.IsFalse(resultIDs.Contains(block.ItemId), $"Block {block.BlockName} (ID {block.ItemId}) should not be included when not executing.");
            }
        }

        [Test]
        public async Task EncodeToMultiSave_IncludesOnlyBlocksWithIncludeInSavesTrueAndExecuting()
        {
            await Task.Delay(100);
            IList<Block> allBlocks = flowchart.GetComponents<Block>();
            // Find all blocks that are both executing and have IncludeInSaves == true
            var includedBlocks = allBlocks.Where(b => b.IncludeInSaves && b.IsExecuting()).ToList();
            Assume.That(includedBlocks.Count > 0, "Test scene needs at least one executing Block with IncludeInSaves == true");

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(flowchart);
            var resultIDs = result.Select(b => b.ItemId).ToList();

            Assert.AreEqual(includedBlocks.Count, result.Count, "Encoded the wrong number of Blocks.");
            foreach (var block in includedBlocks)
            {
                Assert.IsTrue(resultIDs.Contains(block.ItemId), $"Block {block.BlockName} (ID {block.ItemId}) should be included.");
            }
        }

        [Test]
        public async Task EncodeToMultiSave_NoOutputWhenNoBlocksToSave()
        {
            await Task.Delay(100);

            GameObject dummy = new GameObject("FlowchartNoBlocks");
            Flowchart dummyFlowchart = dummy.AddComponent<Flowchart>();
            IList<Block> blocksFound = dummyFlowchart.GetComponents<Block>();
            Assume.That(blocksFound.Count == 0, "Adding Dummy Flowchart added a Block");

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(dummyFlowchart);
            Assert.IsTrue(result.Count == 0, "Created block save data from Flowchart with no Blocks");
        }

        [Test]
        public async Task EncodeToMultiSave_NoOutputWhenAllBlocksExcluded()
        {
            await Task.Delay(100);

            IList<Block> allBlocks = flowchart.GetComponents<Block>();
            foreach (var elem in allBlocks)
            {
                elem.IncludeInSaves = false;
            }

            IList<BlockSaveData> result = blockSaveCodec.EncodeToMultiSave(flowchart);
            Assert.IsTrue(result.Count == 0, "Created block save data from Flowchart with all Blocks set to NOT be included");
        }

    }
}