using Amanita.SaveSys;
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;
========
using NUnit.Framework;
using System.Collections;
using UnityEngine;
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
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
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
        }

        protected Block block;


<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
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
========
        protected BlockSaveData blockSaveData = null;
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs

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
        public virtual IEnumerator CorrectBlockIDDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData blockSaveData = new(block);
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            Assert.AreEqual(block.ActiveCommand.ItemId, blockSaveData.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIDDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexEncoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData blockSaveData = new(block);
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            Assert.AreEqual(block.ActiveCommand.CommandIndex, blockSaveData.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectActiveCommandIndexDEcoded()
        {
            yield return new WaitForSeconds(0.1f);
            Assert.IsNotNull(block.ActiveCommand, "Active command not found in block.");
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData blockSaveData = new(block);
            SerializedSaveData serializedData = blockSaveData.Serialized();
========
            blockSaveData = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = blockSaveData.Serialized();
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
        }

        [UnityTest]
        public virtual IEnumerator CorrectBlockSaveDataSerialized()
        {
            yield return new WaitForSeconds(0.1f);
<<<<<<<< HEAD:Assets/Tests/PlayMode/BlockEncoderTests.cs
            BlockSaveData beforeSerializing = new(block);
            SerializedSaveData serializedData = beforeSerializing.Serialized();
========
            BlockSaveData beforeSerializing = blockSaveCodec.EncodeToSave(block);
            SaveDataUnit serializedData = beforeSerializing.Serialized();
>>>>>>>> newSaveSystem:Assets/Tests/PlayMode/SaveSys/BlockCodecTests.cs
            BlockSaveData deserializedBlock = BlockSaveData.DeserializeFrom(serializedData);
            
            Assert.IsNotNull(deserializedBlock, "Deserialized Block is null.");
            Assert.AreEqual(block.ActiveCommand.CommandIndex, deserializedBlock.ActiveCommandIndex, "Active command index mismatch.");
            Assert.AreEqual(block.ActiveCommand.ItemId, deserializedBlock.ActiveCommandId, "Active command ID mismatch.");
            Assert.AreEqual(block.ItemId, deserializedBlock.ItemId, "Serialized Block ID mismatch.");
            Assert.AreEqual(block.BlockName, deserializedBlock.BlockName, "Serialized Block name mismatch.");
        }
    }
}