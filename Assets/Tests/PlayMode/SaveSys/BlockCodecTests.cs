using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
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

        [UnityTest]
        public virtual IEnumerator CorrectBlockID_DEcoded()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandID_ENcoded()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandID_DEcoded()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndex_ENcoded()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndex_DEcoded()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectBlockSaveDataSerialized()
        {
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
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