using Amanita.SaveSys;
using Amanita.VScripting;
using FullSerializer;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using Amanita.FSExt;
using Amanita;

namespace SaveSystemTests
{
    public class FlowchartSaveCodecTests : CommonTestFunctionality
    {
        // Faster: only need scene + flowchart + codecs.
        protected override bool ReqSaveSystem => false;

        // Note that in CommonTestFunctionality, the flowchart save codec is put to use. What
        // we do in this suite is evaluate the results.
        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsUniqueId()
        {
            Assert.AreEqual(flowchart.UniqueId, flowchartSaveData.UniqueId);
        }

        [Test]
        public virtual void FlowchartSaveData_Constructor_SetsFlowchartName()
        {
            Assert.AreEqual(flowchart.name, flowchartSaveData.FlowchartName);
        }

        [UnityTest]
        public virtual IEnumerator SavesRightUniqueId()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            bool rightUniqueIdSaved = flowchart.UniqueId == flowchartSaveData.UniqueId;
            Assert.IsTrue(rightUniqueIdSaved);
        }

        [UnityTest]
        public virtual IEnumerator SavesRightFlowchartName()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            bool rightFlowchartNameSaved = flowchart.name == flowchartSaveData.FlowchartName;
            Assert.IsTrue(rightFlowchartNameSaved);
        }

        [UnityTest]
        public virtual IEnumerator SavesRightNumberOfVariables()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            bool rightVariableCountSaved = flowchart.Variables.Count == flowchartSaveData.SavedVars.Count;
            Assert.IsTrue(rightVariableCountSaved);
        }

        [UnityTest]
        public virtual IEnumerator SavesTheVariablesValuesCorrectly()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            #region Name
            string expectedName = nameVar.Value;
            IList<VariableSaveData> varSaves = flowchartSaveData.SavedVars;
            VariableSaveData relevantSave = varSaves.Where((elem) => elem.Key == nameVar.Key).First();
            bool savedCorrectly = relevantSave.Value == expectedName;
            Assert.IsTrue(savedCorrectly, $"Did not properly save the name var. What was saved: {relevantSave.Value}");
            #endregion

            #region Score
            int expectedScore = scoreVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == scoreVar.Key).First();
            savedCorrectly = relevantSave.Value == scoreVar.Value.ToString();
            Assert.IsTrue(savedCorrectly, $"Did not properly save the score var. What was saved: {relevantSave.Value}");
            #endregion

            #region Is New Player
            bool expectedIsNewPlayer = isNewPlayerVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == isNewPlayerVar.Key).First();
            savedCorrectly = relevantSave.Value == expectedIsNewPlayer.ToString();
            Assert.IsTrue(savedCorrectly, $"Did not properly save the isNewPlayer var. What was saved: {relevantSave.Value}");
            #endregion

            #region Fastest Time
            float expectedFastestTime = fastestTimeVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == fastestTimeVar.Key).First();
            string roundTripFormat = "R";
            savedCorrectly = relevantSave.Value == expectedFastestTime.ToString(roundTripFormat);
            Assert.IsTrue(savedCorrectly, $"Did not properly save the fastest time. What was saved: {relevantSave.Value}");
            #endregion

            #region ThreeDPos
            Vector3 expectedThreeDPos = threeDPosVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == threeDPosVar.Key).First();
            string[] parts = relevantSave.Value.Split(',');
            float.TryParse(parts[0], out float xVal);
            float.TryParse(parts[1], out float yVal);
            float.TryParse(parts[2], out float zVal);
            Vector3 decoded = new Vector3(xVal, yVal, zVal);
            savedCorrectly = decoded == expectedThreeDPos;
            #endregion

            #region TwoDPos
            Vector2 expectedTwoDPos = twoDPosVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == twoDPosVar.Key).First();
            fsSerializer serializer = AmanitaManager.DefaultSerializer;
            lock (serializer)
            {
                Vector2State vec2State = serializer.FromJson<Vector2State>(relevantSave.Value);
                Vector2 decodedTwo = vec2State.ToVector2();
                savedCorrectly = decodedTwo == expectedTwoDPos;
            }
            Assert.IsTrue(savedCorrectly, $"Did not properly save the twoDPos var. What was saved: {relevantSave.Value}");
            #endregion

            #region Other String
            string expectedOtherString = stringVar.Value;
            relevantSave = varSaves.Where((elem) => elem.Key == stringVar.Key).First();
            savedCorrectly = relevantSave.Value == expectedOtherString;
            Assert.IsTrue(savedCorrectly, $"Did not properly save the string var. What was saved: {relevantSave.Value}");
            #endregion

            #region Transform
            Transform expectedTransform = transformVar.Value;
            TransformState expectedTFormState = TransformState.From(expectedTransform);
            relevantSave = varSaves.Where((elem) => elem.Key == transformVar.Key).First();
            TransformState decodedTfState = new TransformState();
            fsData data = fsJsonParser.Parse(relevantSave.Value);
            serializer.TryDeserialize(data, ref decodedTfState);
            savedCorrectly = expectedTFormState.Equals(decodedTfState);
            #endregion

            Assert.IsTrue(savedCorrectly, $"Did not properly save the transform var. " +
                $"What was saved:\n{decodedTfState}\n\n" +
                $"What was expected:\n{expectedTFormState}");
        }

        [UnityTest]
        public virtual IEnumerator SavesRightNumberOfBlocks()
        {
            yield return new WaitForSeconds(0.1f); // Wait for the flowchart to initialize
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            IList<Block> blocksToSave = (from elem in flowchart.GetExecutingBlocks()
                                         where elem.IncludeInSaves
                                         select elem).ToList();

            bool rightBlockCountSaved = blocksToSave.Count == flowchartSaveData.SavedBlocks.Count;
            Assert.IsTrue(rightBlockCountSaved);
        }
    }
}