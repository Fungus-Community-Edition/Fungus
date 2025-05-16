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
            numericEncoder = EncoderRegistry.GetEncoder(nameof(IntegerVariable));
            vectorEncoder = EncoderRegistry.GetEncoder(nameof(Vector2Variable));
        }

        protected virtual void PrepScene()
        {
            varStateTestPrefab = Resources.Load<GameObject>(toVarStateTests);
            varStateTestScene = UnityObject.Instantiate(varStateTestPrefab);
            flowchart = varStateTestScene.GetComponentInChildren<Flowchart>();
            PrepVars();
        }

        protected GameObject varStateTestPrefab;
        protected GameObject varStateTestScene;

        protected Flowchart flowchart;

        protected virtual void PrepVars()
        {
            nameVar = (StringVariable)flowchart.GetVariable("name");
            scoreVar = (IntegerVariable)flowchart.GetVariable("score");
            newPlayerVar = (BooleanVariable)flowchart.GetVariable("newPlayer");
            fastestTimeVar = (FloatVariable)flowchart.GetVariable("fastestTimeInSeconds");
            threeDPosVar = (Vector3Variable)flowchart.GetVariable("threeDPos");
            twoDPosVar = (Vector2Variable)flowchart.GetVariable("twoDPos");
        }

        protected StringVariable nameVar = null;
        protected IntegerVariable scoreVar = null;
        protected BooleanVariable newPlayerVar = null;
        protected FloatVariable fastestTimeVar = null;
        protected Vector3Variable threeDPosVar = null;
        protected Vector2Variable twoDPosVar = null;

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

        protected IVarEncoder numericEncoder, vectorEncoder;


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
        public virtual void FlowchartVars_NumericsSerializedProperly()
        {
            // Arrange: set up the variables to be saved
            int scoreBefore = scoreVar.Value;
            float fastestTimeBefore = fastestTimeVar.Value;
            // Act: create a new FlowchartSaveData object
            FlowchartSaveData newData = new FlowchartSaveData(flowchart);

        }

        [Test]
        public virtual void NumericEncoder_EncodingWorks()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;
            string expectedEncodedScoreStr = expectedScore.ToString();
            string expectedEncodedFastestTimeStr = expectedFastestTime.ToString(roundTripFormat);

            string encodedScoreStr = numericEncoder.Encode(scoreVar);
            string encodedFastestTimeStr = numericEncoder.Encode(fastestTimeVar);

            bool encodedScoreSuccess = expectedEncodedScoreStr.Equals(encodedScoreStr);
            bool encodedFastestTimeSuccess = expectedEncodedFastestTimeStr.Equals(encodedFastestTimeStr);

            bool success = encodedScoreSuccess && encodedFastestTimeSuccess;
            Assert.IsTrue(success);
        }

        protected static string roundTripFormat = "R";

        [Test]
        public virtual void NumericEncoder_DEcodingWorks()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            string encodedScoreStr = numericEncoder.Encode(scoreVar);
            string encodedFastestTimeStr = numericEncoder.Encode(fastestTimeVar);

            // Alter the values to help us make sure that the encoding and decoding works
            scoreVar.Value += 123;
            fastestTimeVar.Value += 3429785;

            numericEncoder.Decode(scoreVar, encodedScoreStr);
            numericEncoder.Decode(fastestTimeVar, encodedFastestTimeStr);

            bool scoreEncodeSuccess = expectedScore.Equals(scoreVar.Value);
            bool fastestTimeEncodeSuccess = expectedFastestTime.Equals(fastestTimeVar.Value);
            bool success = scoreEncodeSuccess && fastestTimeEncodeSuccess;
            Assert.IsTrue(success);

        }

        [Test]
        public virtual void VectorEncoder_EncodingWorks()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorEncoder.Encode(twoDPosVar);
            string encodedThreeDPosStr = vectorEncoder.Encode(threeDPosVar);

            bool encodedTwoDPosSuccess = expectedEncodedTwoDPosStr.Equals(encodedTwoDPosStr);
            bool encodedThreeDPosSuccess = expectedEncodedThreeDPosStr.Equals(encodedThreeDPosStr);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;

            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorEncoder_DECodingWorks()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorEncoder.Encode(twoDPosVar);
            string encodedThreeDPosStr = vectorEncoder.Encode(threeDPosVar);

            twoDPosVar.Value += Vector2.right * 123;
            threeDPosVar.Value += Vector3.right * 3429785;

            vectorEncoder.Decode(twoDPosVar, encodedTwoDPosStr);
            vectorEncoder.Decode(threeDPosVar, encodedThreeDPosStr);

            bool encodedTwoDPosSuccess = expectedTwoDPos.Equals(twoDPosVar.Value);
            bool encodedThreeDPosSuccess = expectedThreeDPos.Equals(threeDPosVar.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;
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