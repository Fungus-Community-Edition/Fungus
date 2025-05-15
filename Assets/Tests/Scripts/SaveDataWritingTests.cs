using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using Fungus;
using System.Collections.Generic;
using System;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class SaveDataWritingTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
            PrepMetaData();
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            newPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;
        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable newPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;

        protected virtual void PrepMetaData()
        {
            expectedTypeName = metaData.TypeName;
            expectedSaveVer = 3.32789f;
            expectedTimeStamp = DateTime.UtcNow.ToString("o");

            metaData.SaveVersion = expectedSaveVer;
            metaData.UTCTimeStamp = expectedTimeStamp;

            serializedMetaData = metaData.Serialized();
            deserializedMetaData = SaveMetaData.DeserializeFrom(serializedMetaData);
        }

        SaveMetaData metaData = new SaveMetaData();
        protected SerializedSaveData serializedMetaData;
        protected SaveMetaData deserializedMetaData;
        protected string expectedTypeName, expectedTimeStamp;
        float expectedSaveVer;


        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.Destroy(varStateTestScene);
        }

        [Test]
        public virtual void Metadata_TypeNameSerializedProperly()
        {
            Assert.AreEqual(serializedMetaData.DataType, expectedTypeName);
        }

        [Test]
        public virtual void Metadata_MainFieldsSerializedProperly()
        {
            Debug.Log($"Checking if the main metadata fields were serialized properly.");
            bool success = metaData.Equals(deserializedMetaData);
            Assert.IsTrue(success);
        }

        [Test]
        [Ignore("")]
        public void SaveAndLoad_FlowchartVariables_ExpectedValuesSaved()
        {
            string nameBefore = nameVar.Value;
            int scoreBefore = scoreVar.Value;
            bool newPlayerBefore = newPlayerVar.Value;
            float fastestTimeBefore = fastestTimeVar.Value;

            FlowchartSaveData newData = new FlowchartSaveData(flowchart);

            // Assert: the values are written as expected
            Assert.AreEqual(nameBefore, newData);
        }

        
    }
}