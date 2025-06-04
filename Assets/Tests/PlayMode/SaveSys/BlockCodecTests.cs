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
            PrepVars();
            block = flowchart.FindBlock("TestBlock");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
        }

        protected Block block;

        protected virtual void PrepVars()
        {
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            newPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (Vector3Variable)flowchart.GetVariable("threeDPos");
            twoDPosVar = (Vector2Variable)flowchart.GetVariable("twoDPos");

            stringVar = flowchart.gameObject.AddComponent<StringVariable>();
            stringVar.Value = "Hello, World!";
            flowchart.Variables.Add(stringVar);

            transformVar = (TransformVariable)flowchart.GetVariable("someTrans");
        }

        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable newPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;
        protected Vector3Variable threeDPosVar = null;
        protected Vector2Variable twoDPosVar = null;
        protected StringVariable stringVar = null;
        protected TransformVariable transformVar = null;
        protected BlockSaveData blockSaveData = null;

        [Test]
        public virtual void CorrectBlockIDEncoded()
        {
            Assert.AreEqual(block.ItemId, blockSaveData.ItemId, "Block ID mismatch.");
        }

        [Test]
        public virtual void CorrectBlockNameEncoded()
        {
            Assert.AreEqual(block.BlockName, blockSaveData.BlockName, "Block name mismatch.");
        }

        [Test]
        public virtual void CorrectBlockNameDEcoded()
        {
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectBlockIDDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexDEcoded()
        {
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