using NUnit.Framework;
using UnityEngine;
using System.Collections;
using Amanita.SaveSys;
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;
using UnityEngine.TestTools;

namespace Amanita.SaveSystemTests
{
    public class BlockEncoderTests
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
            PrepVars();
            block = flowchart.FindBlock("TestBlock");
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;
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

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void CorrectBlockIDEncoded()
        {
            BlockSaveData blockSaveData = new(block);
            Assert.AreEqual(block.ItemId, blockSaveData.ItemId, "Block ID mismatch.");
        }

        [Test]
        public virtual void CorrectBlockNameEncoded()
        {
            BlockSaveData blockSaveData = new(block);
            Assert.AreEqual(block.BlockName, blockSaveData.BlockName, "Block name mismatch.");
        }

        [Test]
        public virtual void CorrectBlockNameDEcoded()
        {
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            BlockSaveData blockSaveData = new(block);
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            BlockSaveData blockSaveData = new(block);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectBlockSaveDataSerialized()
        {
            yield return new WaitForSeconds(0.1f);
            BlockSaveData beforeSerializing = new(block);
            SerializedSaveData serializedData = beforeSerializing.Serialized();
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            
            Assert.IsNotNull(deserializedBlock, "Deserialized Block is null.");
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }
    }
}