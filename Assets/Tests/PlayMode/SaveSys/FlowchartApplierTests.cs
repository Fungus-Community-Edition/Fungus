using NUnit.Framework;
using UnityEngine;
using Amanita.SaveSys;
using UnityObject = UnityEngine.Object;
using System.Collections;
using UnityEngine.TestTools;
using System.Threading.Tasks;

namespace Amanita.SaveSystemTests
{
    public class FlowchartApplierTests : CommonTestFunctionality
    {

        [UnityTest]
        public virtual IEnumerator AppliesVarStates()
        {
            yield return CommonSetup();
            string expectedNameVarValue = nameVar.Value;
            int expectedScoreVarValue = scoreVar.Value;
            bool expectedNewPlayerVarValue = isNewPlayerVar.Value;
            float expectedFastestTimeVarValue = fastestTimeVar.Value;
            Vector3 expectedThreeDPosVarValue = threeDPosVar.Value;
            Vector2 expectedTwoDPosVarValue = twoDPosVar.Value;
            string expectedStringVarValue = stringVar.Value;
            Transform expectedTransformVarValue = transformVar.Value;

            // Change the states of the vars so that when loading the save data, we
            // can see if they were applied correctly.
            nameVar.Value = "Not Hello, World!";
            scoreVar.Value = 0;
            isNewPlayerVar.Value = false;
            fastestTimeVar.Value = -10.0f;
            threeDPosVar.Value = new Vector3(0.0f, 0.0f, 0.0f);
            twoDPosVar.Value = new Vector2(0.0f, 0.0f);
            stringVar.Value = "Not Hello, World!";
            transformVar.Value = null;

            Task applyTask = flowchartApplier.ApplyMulti(new FlowchartSaveData[] { flowchartSaveData });
            yield return new WaitUntil(() => applyTask.IsCompleted);

            bool appliedCorrectName = nameVar.Value == expectedNameVarValue;
            bool appliedCorrectScore = scoreVar.Value == expectedScoreVarValue;
            bool appliedCorrectNewPlayer = isNewPlayerVar.Value == expectedNewPlayerVarValue;
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
            yield return CommonSetup();
            yield return new WaitForSeconds(0.1f);
            flowchartSaveData = flowchartSaveCodec.EncodeToSave(flowchart);
            Task applyTask = flowchartApplier.ApplyMulti(new FlowchartSaveData[] { flowchartSaveData });
            yield return new WaitUntil(() => applyTask.IsCompleted);
            yield return new WaitForSeconds(0.1f);
            // The block should be executed at this time

            Block testBlock = flowchart.FindBlock("TestBlock");
            bool blockExecuted = testBlock.IsExecuting();
            Assert.IsTrue(blockExecuted, "FlowchartApplier did not apply the block states correctly.");

        }
    }
}