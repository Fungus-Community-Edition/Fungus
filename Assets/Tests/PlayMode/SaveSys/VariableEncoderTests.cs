using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class VariableEncoderTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
            PrepEncoders();
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

        protected virtual void PrepEncoders()
        {
            numericEncoder = EncoderRegistry.GetEncoder(nameof(IntegerVariable));
            booleanEncoder = EncoderRegistry.GetEncoder(nameof(BooleanVariable));
            vectorEncoder = EncoderRegistry.GetEncoder(nameof(Vector2Variable));
            colorEncoder = EncoderRegistry.GetEncoder(nameof(ColorVariable));
            stringEncoder = EncoderRegistry.GetEncoder(nameof(StringVariable));
            transformEncoder = EncoderRegistry.GetEncoder(nameof(TransformVariable));
        }

        protected IVarEncoder numericEncoder, booleanEncoder, vectorEncoder, colorEncoder, stringEncoder, transformEncoder;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void NumericEncoder_EncodingWorks_String()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;
            string expectedEncodedScoreStr = expectedScore.ToString();
            string expectedEncodedFastestTimeStr = expectedFastestTime.ToString(roundTripFormat);

            string encodedScoreStr = numericEncoder.EncodeToString(scoreVar);
            string encodedFastestTimeStr = numericEncoder.EncodeToString(fastestTimeVar);

            bool encodedScoreSuccess = expectedEncodedScoreStr.Equals(encodedScoreStr);
            bool encodedFastestTimeSuccess = expectedEncodedFastestTimeStr.Equals(encodedFastestTimeStr);

            bool success = encodedScoreSuccess && encodedFastestTimeSuccess;
            Assert.IsTrue(success);
        }

        protected static string roundTripFormat = "R";

        [Test]
        public virtual void NumericEncoder_EncodingWorks_VarSaveData()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            string expectedEncodedScoreStr = expectedScore.ToString();
            string expectedEncodedFastestTimeStr = expectedFastestTime.ToString(roundTripFormat);

            VariableSaveData encodedScoreVarData = numericEncoder.EncodeToSave(scoreVar);
            VariableSaveData encodedFastestTimeVarData = numericEncoder.EncodeToSave(fastestTimeVar);

            bool encodedScoreSuccess = expectedEncodedScoreStr.Equals(encodedScoreVarData.Value);
            bool encodedFastestTimeSuccess = expectedEncodedFastestTimeStr.Equals(encodedFastestTimeVarData.Value);
            bool success = encodedScoreSuccess && encodedFastestTimeSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void NumericEncoder_DEcodingWorks_String()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            string encodedScoreStr = numericEncoder.EncodeToString(scoreVar);
            string encodedFastestTimeStr = numericEncoder.EncodeToString(fastestTimeVar);

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
        public virtual void NumericEncoder_DEcodingWorks_VarSaveData()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            VariableSaveData encodedScoreVarData = numericEncoder.EncodeToSave(scoreVar);
            VariableSaveData encodedFastestTimeData = numericEncoder.EncodeToSave(fastestTimeVar);

            // Alter the values to help us make sure that the encoding and decoding works
            scoreVar.Value += 123;
            fastestTimeVar.Value += 3429785;

            numericEncoder.Decode(scoreVar, encodedScoreVarData);
            numericEncoder.Decode(fastestTimeVar, encodedFastestTimeData);

            bool scoreEncodeSuccess = expectedScore.Equals(scoreVar.Value);
            bool fastestTimeEncodeSuccess = expectedFastestTime.Equals(fastestTimeVar.Value);
            bool success = scoreEncodeSuccess && fastestTimeEncodeSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void BooleanEncoder_EncodingWorks_String()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string expectedEncodedNewPlayerStr = expectedNewPlayer.ToString();
            string encodedNewPlayerStr = booleanEncoder.EncodeToString(newPlayerVar);
            bool encodedNewPlayerSuccess = expectedEncodedNewPlayerStr.Equals(encodedNewPlayerStr);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanEncoder_DEcodingWorks_String()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string encodedNewPlayerStr = booleanEncoder.EncodeToString(newPlayerVar);
            newPlayerVar.Value = !newPlayerVar.Value; // Change the value to make sure we decode correctly
            booleanEncoder.Decode(newPlayerVar, encodedNewPlayerStr);
            bool encodedNewPlayerSuccess = expectedNewPlayer.Equals(newPlayerVar.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanEncoder_EncodingWorks_VarSaveData()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string expectedEncodedNewPlayerStr = expectedNewPlayer.ToString();
            VariableSaveData encodedNewPlayerData = booleanEncoder.EncodeToSave(newPlayerVar);
            bool encodedNewPlayerSuccess = expectedEncodedNewPlayerStr.Equals(encodedNewPlayerData.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanEncoder_DEcodingWorks_VarSaveData()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            VariableSaveData encodedNewPlayerData = booleanEncoder.EncodeToSave(newPlayerVar);
            newPlayerVar.Value = !newPlayerVar.Value; // Change the value to make sure we decode correctly
            booleanEncoder.Decode(newPlayerVar, encodedNewPlayerData);
            bool encodedNewPlayerSuccess = expectedNewPlayer.Equals(newPlayerVar.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void VectorEncoder_EncodingWorks_String()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorEncoder.EncodeToString(twoDPosVar);
            string encodedThreeDPosStr = vectorEncoder.EncodeToString(threeDPosVar);

            bool encodedTwoDPosSuccess = expectedEncodedTwoDPosStr.Equals(encodedTwoDPosStr);
            bool encodedThreeDPosSuccess = expectedEncodedThreeDPosStr.Equals(encodedThreeDPosStr);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;

            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorEncoder_EncodingWorks_VarSaveData()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            VariableSaveData encodedTwoDPosData = vectorEncoder.EncodeToSave(twoDPosVar);
            VariableSaveData encodedThreeDPosData = vectorEncoder.EncodeToSave(threeDPosVar);

            bool encodedTwoDPosSuccess = expectedEncodedTwoDPosStr.Equals(encodedTwoDPosData.Value);
            bool encodedThreeDPosSuccess = expectedEncodedThreeDPosStr.Equals(encodedThreeDPosData.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;

            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorEncoder_DECodingWorks_String()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorEncoder.EncodeToString(twoDPosVar);
            string encodedThreeDPosStr = vectorEncoder.EncodeToString(threeDPosVar);

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
        public virtual void VectorEncoder_DECodingWorks_VarSaveData()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            VariableSaveData twoDPosData = vectorEncoder.EncodeToSave(twoDPosVar);
            VariableSaveData threeDPosData = vectorEncoder.EncodeToSave(threeDPosVar);

            twoDPosVar.Value += Vector2.right * 123;
            threeDPosVar.Value += Vector3.right * 3429785;

            vectorEncoder.Decode(twoDPosVar, twoDPosData);
            vectorEncoder.Decode(threeDPosVar, threeDPosData);

            bool encodedTwoDPosSuccess = expectedTwoDPos.Equals(twoDPosVar.Value);
            bool encodedThreeDPosSuccess = expectedThreeDPos.Equals(threeDPosVar.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void ColorEncoder_EncodingWorks_String()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;

            string expectedEncodedColorStr = $"{expectedColor.r},{expectedColor.g},{expectedColor.b},{expectedColor.a}";
            string encodedColorStr = colorEncoder.EncodeToString(colorVar);
            bool encodedColorSuccess = expectedEncodedColorStr.Equals(encodedColorStr);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorEncoder_EncodingWorks_VarSaveData()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;
            string expectedEncodedColorStr = $"{expectedColor.r},{expectedColor.g},{expectedColor.b},{expectedColor.a}";
            VariableSaveData encodedColorVarData = colorEncoder.EncodeToSave(colorVar);
            bool encodedColorSuccess = expectedEncodedColorStr.Equals(encodedColorVarData.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorEncoder_DEcodingWorks_String()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;

            string encodedColorStr = colorEncoder.EncodeToString(colorVar);
            colorVar.Value += new Color(0.1f, 0.1f, 0.1f, 0.1f);
            colorEncoder.Decode(colorVar, encodedColorStr);
            bool encodedColorSuccess = expectedColor.Equals(colorVar.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorEncoder_DEcodingWorks_VarSaveData()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;
            VariableSaveData encodedColorVarData = colorEncoder.EncodeToSave(colorVar);
            colorVar.Value += new Color(0.1f, 0.1f, 0.1f, 0.1f);
            colorEncoder.Decode(colorVar, encodedColorVarData);
            bool encodedColorSuccess = expectedColor.Equals(colorVar.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void StringEncoder_EncodingWorks_String()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            string encodedString = stringEncoder.EncodeToString(stringVar);
            bool encodedStringSuccess = expectedString.Equals(encodedString);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringEncoder_EncodingWorks_VarSaveData()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            VariableSaveData encodedStringVarData = stringEncoder.EncodeToSave(stringVar);
            bool encodedStringSuccess = expectedString.Equals(encodedStringVarData.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringEncoder_DEcodingWorks_String()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            string encodedString = stringEncoder.EncodeToString(stringVar);
            stringVar.Value += " Good bye, cruel world!";
            stringEncoder.Decode(stringVar, encodedString);
            bool encodedStringSuccess = expectedString.Equals(stringVar.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringEncoder_DEcodingWorks_VarSaveData()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            VariableSaveData encodedStringVarData = stringEncoder.EncodeToSave(stringVar);
            stringVar.Value += " Good bye, cruel world!";
            stringEncoder.Decode(stringVar, encodedStringVarData);
            bool encodedStringSuccess = expectedString.Equals(stringVar.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void TransformEncoder_EncodingWorks_String()
        {
            Transform expectedTrans = transformVar.Value;
            TransformState expectedState = TransformState.From(expectedTrans);
            string expectedEncodedTransStr = JsonUtility.ToJson(expectedState);
            string encodedTransStr = transformEncoder.EncodeToString(transformVar);
            bool encodedTransSuccess = expectedEncodedTransStr.Equals(encodedTransStr);
            Assert.IsTrue(encodedTransSuccess);
        }

        [Test]
        public virtual void TransformEncoder_EncodingWorks_VarSaveData()
        {
            Transform expectedTrans = transformVar.Value;
            TransformState expectedState = TransformState.From(expectedTrans);
            string expectedEncodedTransStr = JsonUtility.ToJson(expectedState);
            VariableSaveData encodedTransVarData = transformEncoder.EncodeToSave(transformVar);
            bool encodedTransSuccess = expectedEncodedTransStr.Equals(encodedTransVarData.Value);
            Assert.IsTrue(encodedTransSuccess);
        }

        [Test]
        public virtual void TransformEncoder_DEcodingWorks_String()
        {
            Transform expectedTrans = transformVar.Value; // Should NOT be null at this point
            string expectedName = expectedTrans.name;
            SaveIdentifier identifier = expectedTrans.GetComponent<SaveIdentifier>();
            string expectedUniqueID = null;
            if (identifier != null)
            {
                expectedUniqueID = identifier.UniqueID;
            }
            Vector3 expectedPos = expectedTrans.position;
            Quaternion expectedRot = expectedTrans.rotation;
            Vector3 expectedScale = expectedTrans.localScale;

            string encodedTransStr = transformEncoder.EncodeToString(transformVar);
            transformVar.Value.position += Vector3.right * 123;
            transformVar.Value.rotation *= Quaternion.Euler(0, 90, 0);
            transformVar.Value.localScale += Vector3.one * 0.5f;
            transformVar.Value = null;

            transformEncoder.Decode(transformVar, encodedTransStr);
            // Part of the decoding process is applying the position, rotation and such
            // to the transform. Thus, we won't need to apply it here.
            Transform decodedTrans = transformVar.Value;

            bool encodedTransSuccess = expectedTrans == decodedTrans;
            bool encodedPosSuccess = expectedPos.Equals(decodedTrans.position);
            bool encodedRotSuccess = expectedRot.Equals(decodedTrans.rotation);
            bool encodedScaleSuccess = expectedScale.Equals(decodedTrans.localScale);
            bool encodedNameSuccess = expectedName.Equals(decodedTrans.name);
            bool encodedUniqueIDSuccess = true;
            
            if (identifier != null)
            {
                encodedUniqueIDSuccess = expectedUniqueID.Equals(identifier.UniqueID);
            }
            else
            {
                encodedUniqueIDSuccess = decodedTrans.GetComponent<SaveIdentifier>() == null;
            }

            bool success = encodedTransSuccess && encodedPosSuccess && encodedRotSuccess && encodedScaleSuccess && encodedNameSuccess && encodedUniqueIDSuccess;
            Assert.IsTrue(success);

        }

        [Test]
        public virtual void TransformEncoder_DEcodingWorks_VarSaveData()
        {
            Transform expectedTrans = transformVar.Value; // Should NOT be null at this point
            string expectedName = expectedTrans.name;
            SaveIdentifier identifier = expectedTrans.GetComponent<SaveIdentifier>();
            string expectedUniqueID = null;
            if (identifier != null)
            {
                expectedUniqueID = identifier.UniqueID;
            }
            Vector3 expectedPos = expectedTrans.position;
            Quaternion expectedRot = expectedTrans.rotation;
            Vector3 expectedScale = expectedTrans.localScale;

            VariableSaveData encodedTransVarData = transformEncoder.EncodeToSave(transformVar);

            transformVar.Value.position += Vector3.right * 123;
            transformVar.Value.rotation *= Quaternion.Euler(0, 90, 0);
            transformVar.Value.localScale += Vector3.one * 0.5f;
            transformVar.Value = null;

            transformEncoder.Decode(transformVar, encodedTransVarData);
            // Part of the decoding process is applying the position, rotation and such
            // to the transform. Thus, we won't need to apply it here.
            Transform decodedTrans = transformVar.Value;

            bool encodedTransSuccess = expectedTrans == decodedTrans;
            bool encodedPosSuccess = expectedPos.Equals(decodedTrans.position);
            bool encodedRotSuccess = expectedRot.Equals(decodedTrans.rotation);
            bool encodedScaleSuccess = expectedScale.Equals(decodedTrans.localScale);
            bool encodedNameSuccess = expectedName.Equals(decodedTrans.name);
            bool encodedUniqueIDSuccess = true;

            if (identifier != null)
            {
                encodedUniqueIDSuccess = expectedUniqueID.Equals(identifier.UniqueID);
            }
            else
            {
                encodedUniqueIDSuccess = decodedTrans.GetComponent<SaveIdentifier>() == null;
            }

            bool success = encodedTransSuccess && encodedPosSuccess && encodedRotSuccess && encodedScaleSuccess && encodedNameSuccess && encodedUniqueIDSuccess;
            Assert.IsTrue(success);
        }
    }
}