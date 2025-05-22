using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;
using System.Collections;
using UnityEngine.TestTools;

namespace Amanita.SaveSystemTests
{
    public class FlowchartApplierTests
    {
        protected string toVarStateTests = "ScenePrefabs/VarStateTests";

        [SetUp]
        public virtual void DoSetUp()
        {
            PrepScene();
            flowchartApplier = ScriptableObject.CreateInstance<FlowchartApplier>();
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

        protected FlowchartApplier flowchartApplier;

        [TearDown]
        public virtual void DoTearDown()
        {
            UnityObject.DestroyImmediate(varStateTestScene);
        }

        [Test]
        public virtual void AppliesVarStates()
        {
            FlowchartSaveData saveData = new FlowchartSaveData(flowchart);

            string expectedNameVarValue = nameVar.Value;
            int expectedScoreVarValue = scoreVar.Value;
            bool expectedNewPlayerVarValue = newPlayerVar.Value;
            float expectedFastestTimeVarValue = fastestTimeVar.Value;
            Vector3 expectedThreeDPosVarValue = threeDPosVar.Value;
            Vector2 expectedTwoDPosVarValue = twoDPosVar.Value;
            string expectedStringVarValue = stringVar.Value;
            Transform expectedTransformVarValue = transformVar.Value;

            // Change the states of the vars so that when loading the save data, we
            // can see if they were applied correctly.
            nameVar.Value = "Not Hello, World!";
            scoreVar.Value = 0;
            newPlayerVar.Value = false;
            fastestTimeVar.Value = -10.0f;
            threeDPosVar.Value = new Vector3(0.0f, 0.0f, 0.0f);
            twoDPosVar.Value = new Vector2(0.0f, 0.0f);
            stringVar.Value = "Not Hello, World!";
            transformVar.Value = null;

            flowchartApplier.Apply(new FlowchartSaveData[] { saveData });

            bool appliedCorrectName = nameVar.Value == expectedNameVarValue;
            bool appliedCorrectScore = scoreVar.Value == expectedScoreVarValue;
            bool appliedCorrectNewPlayer = newPlayerVar.Value == expectedNewPlayerVarValue;
            bool appliedCorrectFastestTime = fastestTimeVar.Value == expectedFastestTimeVarValue;
            bool appliedCorrectThreeDPos = threeDPosVar.Value == expectedThreeDPosVarValue;
            bool appliedCorrectTwoDPos = twoDPosVar.Value == expectedTwoDPosVarValue;
            bool appliedCorrectString = stringVar.Value == expectedStringVarValue;
            bool appliedCorrectTransform = transformVar.Value == expectedTransformVarValue;
            bool success = appliedCorrectName && appliedCorrectScore && appliedCorrectNewPlayer &&
                appliedCorrectFastestTime && appliedCorrectThreeDPos && appliedCorrectTwoDPos &&
                appliedCorrectString && appliedCorrectTransform;
            Assert.IsTrue(success, "FlowchartApplier did not apply the variable states correctly.");
        }

        [UnityTest]
        public virtual IEnumerator ReexecutesBlocks()
        {
            yield return new WaitForSeconds(0.1f);
            FlowchartSaveData saveData = new FlowchartSaveData(flowchart);
            flowchartApplier.Apply(new FlowchartSaveData[] { saveData });
            yield return new WaitForSeconds(0.1f);
            // The block should be executed at this time

            Block testBlock = flowchart.FindBlock("TestBlock");
            bool blockExecuted = testBlock.IsExecuting();
            Assert.IsTrue(blockExecuted, "FlowchartApplier did not apply the block states correctly.");

        }
    }
}