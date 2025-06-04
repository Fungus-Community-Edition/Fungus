using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class VariableCodecTests : CommonTestFunctionality
    {
        [OneTimeSetUp]
        public override void DoOneTimeSetUp()
        {
            base.DoOneTimeSetUp();
            PrepCodecs();
        }

        protected virtual void PrepCodecs()
        {
            numericCodec = CodecRegistry.GetCodec(nameof(IntegerVariable));
            booleanCodec = CodecRegistry.GetCodec(nameof(BooleanVariable));
            vectorCodec = CodecRegistry.GetCodec(nameof(Vector2Variable));
            colorCodec = CodecRegistry.GetCodec(nameof(ColorVariable));
            stringCodec = CodecRegistry.GetCodec(nameof(StringVariable));
            transformCodec = CodecRegistry.GetCodec(nameof(TransformVariable));
        }

        protected IVarCodec numericCodec, booleanCodec, vectorCodec, colorCodec, stringCodec, transformCodec;

        [Test]
        public virtual void NumericCodec_EncodingWorks_String()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;
            string expectedEncodedScoreStr = expectedScore.ToString();
            string expectedEncodedFastestTimeStr = expectedFastestTime.ToString(roundTripFormat);

            string encodedScoreStr = numericCodec.EncodeToString(scoreVar);
            string encodedFastestTimeStr = numericCodec.EncodeToString(fastestTimeVar);

            bool encodedScoreSuccess = expectedEncodedScoreStr.Equals(encodedScoreStr);
            bool encodedFastestTimeSuccess = expectedEncodedFastestTimeStr.Equals(encodedFastestTimeStr);

            bool success = encodedScoreSuccess && encodedFastestTimeSuccess;
            Assert.IsTrue(success);
        }

        protected static string roundTripFormat = "R";

        [Test]
        public virtual void NumericCodec_EncodingWorks_VarSaveData()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            string expectedEncodedScoreStr = expectedScore.ToString();
            string expectedEncodedFastestTimeStr = expectedFastestTime.ToString(roundTripFormat);

            VariableSaveData encodedScoreVarData = numericCodec.EncodeToSave(scoreVar);
            VariableSaveData encodedFastestTimeVarData = numericCodec.EncodeToSave(fastestTimeVar);

            bool encodedScoreSuccess = expectedEncodedScoreStr.Equals(encodedScoreVarData.Value);
            bool encodedFastestTimeSuccess = expectedEncodedFastestTimeStr.Equals(encodedFastestTimeVarData.Value);
            bool success = encodedScoreSuccess && encodedFastestTimeSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void NumericCodec_DEcodingWorks_String()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            string encodedScoreStr = numericCodec.EncodeToString(scoreVar);
            string encodedFastestTimeStr = numericCodec.EncodeToString(fastestTimeVar);

            // Alter the values to help us make sure that the encoding and decoding works
            scoreVar.Value += 123;
            fastestTimeVar.Value += 3429785;

            numericCodec.Decode(scoreVar, encodedScoreStr);
            numericCodec.Decode(fastestTimeVar, encodedFastestTimeStr);

            bool scoreEncodeSuccess = expectedScore.Equals(scoreVar.Value);
            bool fastestTimeEncodeSuccess = expectedFastestTime.Equals(fastestTimeVar.Value);
            bool success = scoreEncodeSuccess && fastestTimeEncodeSuccess;
            Assert.IsTrue(success);

        }

        [Test]
        public virtual void NumericCodec_DEcodingWorks_VarSaveData()
        {
            int expectedScore = scoreVar.Value;
            float expectedFastestTime = fastestTimeVar.Value;

            VariableSaveData encodedScoreVarData = numericCodec.EncodeToSave(scoreVar);
            VariableSaveData encodedFastestTimeData = numericCodec.EncodeToSave(fastestTimeVar);

            // Alter the values to help us make sure that the encoding and decoding works
            scoreVar.Value += 123;
            fastestTimeVar.Value += 3429785;

            numericCodec.Decode(scoreVar, encodedScoreVarData);
            numericCodec.Decode(fastestTimeVar, encodedFastestTimeData);

            bool scoreEncodeSuccess = expectedScore.Equals(scoreVar.Value);
            bool fastestTimeEncodeSuccess = expectedFastestTime.Equals(fastestTimeVar.Value);
            bool success = scoreEncodeSuccess && fastestTimeEncodeSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void BooleanCodec_EncodingWorks_String()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string expectedEncodedNewPlayerStr = expectedNewPlayer.ToString();
            string encodedNewPlayerStr = booleanCodec.EncodeToString(newPlayerVar);
            bool encodedNewPlayerSuccess = expectedEncodedNewPlayerStr.Equals(encodedNewPlayerStr);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanCodec_DEcodingWorks_String()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string encodedNewPlayerStr = booleanCodec.EncodeToString(newPlayerVar);
            newPlayerVar.Value = !newPlayerVar.Value; // Change the value to make sure we decode correctly
            booleanCodec.Decode(newPlayerVar, encodedNewPlayerStr);
            bool encodedNewPlayerSuccess = expectedNewPlayer.Equals(newPlayerVar.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanCodec_EncodingWorks_VarSaveData()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            string expectedEncodedNewPlayerStr = expectedNewPlayer.ToString();
            VariableSaveData encodedNewPlayerData = booleanCodec.EncodeToSave(newPlayerVar);
            bool encodedNewPlayerSuccess = expectedEncodedNewPlayerStr.Equals(encodedNewPlayerData.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void BooleanCodec_DEcodingWorks_VarSaveData()
        {
            bool expectedNewPlayer = newPlayerVar.Value;
            VariableSaveData encodedNewPlayerData = booleanCodec.EncodeToSave(newPlayerVar);
            newPlayerVar.Value = !newPlayerVar.Value; // Change the value to make sure we decode correctly
            booleanCodec.Decode(newPlayerVar, encodedNewPlayerData);
            bool encodedNewPlayerSuccess = expectedNewPlayer.Equals(newPlayerVar.Value);
            Assert.IsTrue(encodedNewPlayerSuccess);
        }

        [Test]
        public virtual void VectorCodec_EncodingWorks_String()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorCodec.EncodeToString(twoDPosVar);
            string encodedThreeDPosStr = vectorCodec.EncodeToString(threeDPosVar);

            bool encodedTwoDPosSuccess = expectedEncodedTwoDPosStr.Equals(encodedTwoDPosStr);
            bool encodedThreeDPosSuccess = expectedEncodedThreeDPosStr.Equals(encodedThreeDPosStr);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;

            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorCodec_EncodingWorks_VarSaveData()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            VariableSaveData encodedTwoDPosData = vectorCodec.EncodeToSave(twoDPosVar);
            VariableSaveData encodedThreeDPosData = vectorCodec.EncodeToSave(threeDPosVar);

            bool encodedTwoDPosSuccess = expectedEncodedTwoDPosStr.Equals(encodedTwoDPosData.Value);
            bool encodedThreeDPosSuccess = expectedEncodedThreeDPosStr.Equals(encodedThreeDPosData.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;

            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorCodec_DECodingWorks_String()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            string expectedEncodedTwoDPosStr = $"{expectedTwoDPos.x},{expectedTwoDPos.y}";
            string expectedEncodedThreeDPosStr = $"{expectedThreeDPos.x},{expectedThreeDPos.y},{expectedThreeDPos.z}";

            string encodedTwoDPosStr = vectorCodec.EncodeToString(twoDPosVar);
            string encodedThreeDPosStr = vectorCodec.EncodeToString(threeDPosVar);

            twoDPosVar.Value += Vector2.right * 123;
            threeDPosVar.Value += Vector3.right * 3429785;

            vectorCodec.Decode(twoDPosVar, encodedTwoDPosStr);
            vectorCodec.Decode(threeDPosVar, encodedThreeDPosStr);

            bool encodedTwoDPosSuccess = expectedTwoDPos.Equals(twoDPosVar.Value);
            bool encodedThreeDPosSuccess = expectedThreeDPos.Equals(threeDPosVar.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void VectorCodec_DECodingWorks_VarSaveData()
        {
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            Vector3 expectedThreeDPos = threeDPosVar.Value;

            VariableSaveData twoDPosData = vectorCodec.EncodeToSave(twoDPosVar);
            VariableSaveData threeDPosData = vectorCodec.EncodeToSave(threeDPosVar);

            twoDPosVar.Value += Vector2.right * 123;
            threeDPosVar.Value += Vector3.right * 3429785;

            vectorCodec.Decode(twoDPosVar, twoDPosData);
            vectorCodec.Decode(threeDPosVar, threeDPosData);

            bool encodedTwoDPosSuccess = expectedTwoDPos.Equals(twoDPosVar.Value);
            bool encodedThreeDPosSuccess = expectedThreeDPos.Equals(threeDPosVar.Value);
            bool success = encodedTwoDPosSuccess && encodedThreeDPosSuccess;
            Assert.IsTrue(success);
        }

        [Test]
        public virtual void ColorCodec_EncodingWorks_String()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;

            string expectedEncodedColorStr = $"{expectedColor.r},{expectedColor.g},{expectedColor.b},{expectedColor.a}";
            string encodedColorStr = colorCodec.EncodeToString(colorVar);
            bool encodedColorSuccess = expectedEncodedColorStr.Equals(encodedColorStr);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorCodec_EncodingWorks_VarSaveData()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;
            string expectedEncodedColorStr = $"{expectedColor.r},{expectedColor.g},{expectedColor.b},{expectedColor.a}";
            VariableSaveData encodedColorVarData = colorCodec.EncodeToSave(colorVar);
            bool encodedColorSuccess = expectedEncodedColorStr.Equals(encodedColorVarData.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorCodec_DEcodingWorks_String()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;

            string encodedColorStr = colorCodec.EncodeToString(colorVar);
            colorVar.Value += new Color(0.1f, 0.1f, 0.1f, 0.1f);
            colorCodec.Decode(colorVar, encodedColorStr);
            bool encodedColorSuccess = expectedColor.Equals(colorVar.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void ColorCodec_DEcodingWorks_VarSaveData()
        {
            Color expectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            ColorVariable colorVar = flowchart.gameObject.AddComponent<ColorVariable>();
            colorVar.Value = expectedColor;
            VariableSaveData encodedColorVarData = colorCodec.EncodeToSave(colorVar);
            colorVar.Value += new Color(0.1f, 0.1f, 0.1f, 0.1f);
            colorCodec.Decode(colorVar, encodedColorVarData);
            bool encodedColorSuccess = expectedColor.Equals(colorVar.Value);
            Assert.IsTrue(encodedColorSuccess);
        }

        [Test]
        public virtual void StringCodec_EncodingWorks_String()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            string encodedString = stringCodec.EncodeToString(stringVar);
            bool encodedStringSuccess = expectedString.Equals(encodedString);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringCodec_EncodingWorks_VarSaveData()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            VariableSaveData encodedStringVarData = stringCodec.EncodeToSave(stringVar);
            bool encodedStringSuccess = expectedString.Equals(encodedStringVarData.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringCodec_DEcodingWorks_String()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            string encodedString = stringCodec.EncodeToString(stringVar);
            stringVar.Value += " Good bye, cruel world!";
            stringCodec.Decode(stringVar, encodedString);
            bool encodedStringSuccess = expectedString.Equals(stringVar.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void StringCodec_DEcodingWorks_VarSaveData()
        {
            string expectedString = "Hello, World!";
            stringVar.Value = expectedString;
            VariableSaveData encodedStringVarData = stringCodec.EncodeToSave(stringVar);
            stringVar.Value += " Good bye, cruel world!";
            stringCodec.Decode(stringVar, encodedStringVarData);
            bool encodedStringSuccess = expectedString.Equals(stringVar.Value);
            Assert.IsTrue(encodedStringSuccess);
        }

        [Test]
        public virtual void TransformCodec_EncodingWorks_String()
        {
            Transform expectedTrans = transformVar.Value;
            TransformState expectedState = TransformState.From(expectedTrans);
            string expectedEncodedTransStr = JsonUtility.ToJson(expectedState);
            string encodedTransStr = transformCodec.EncodeToString(transformVar);
            bool encodedTransSuccess = expectedEncodedTransStr.Equals(encodedTransStr);
            Assert.IsTrue(encodedTransSuccess);
        }

        [Test]
        public virtual void TransformCodec_EncodingWorks_VarSaveData()
        {
            Transform expectedTrans = transformVar.Value;
            TransformState expectedState = TransformState.From(expectedTrans);
            string expectedEncodedTransStr = JsonUtility.ToJson(expectedState);
            VariableSaveData encodedTransVarData = transformCodec.EncodeToSave(transformVar);
            bool encodedTransSuccess = expectedEncodedTransStr.Equals(encodedTransVarData.Value);
            Assert.IsTrue(encodedTransSuccess);
        }

        [Test]
        public virtual void TransformCodec_DEcodingWorks_String()
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

            string encodedTransStr = transformCodec.EncodeToString(transformVar);
            transformVar.Value.position += Vector3.right * 123;
            transformVar.Value.rotation *= Quaternion.Euler(0, 90, 0);
            transformVar.Value.localScale += Vector3.one * 0.5f;
            transformVar.Value = null;

            transformCodec.Decode(transformVar, encodedTransStr);
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
        public virtual void TransformCodec_DEcodingWorks_VarSaveData()
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

            VariableSaveData encodedTransVarData = transformCodec.EncodeToSave(transformVar);

            transformVar.Value.position += Vector3.right * 123;
            transformVar.Value.rotation *= Quaternion.Euler(0, 90, 0);
            transformVar.Value.localScale += Vector3.one * 0.5f;
            transformVar.Value = null;

            transformCodec.Decode(transformVar, encodedTransVarData);
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