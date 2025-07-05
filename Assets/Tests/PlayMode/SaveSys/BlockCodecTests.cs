using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
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
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }

        [Test]
        public virtual async Task CorrectBlockID_DEcoded()
        {
            await CommonSetupAsync();
            await Task.Delay(100);
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandID_ENcoded()
        {
            await CommonSetupAsync();
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandID_DEcoded()
        {
            await CommonSetupAsync();
            await Task.Delay(500);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandIndex_ENcoded()
        {
            await CommonSetupAsync();
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [Test]
        public virtual async Task CorrectActiveCommandIndex_DEcoded()
        {
            await CommonSetupAsync();
            await Task.Delay(100);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
        }

        [Test]
        public virtual async Task CorrectBlockSaveDataSerialized()
        {
            await CommonSetupAsync();
            await Task.Delay(100);
            BlockSaveData beforeSerializing = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = beforeSerializing.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            
            Assert.IsNotNull(deserializedBlock, "Deserialized Block is null.");
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }
    }
}