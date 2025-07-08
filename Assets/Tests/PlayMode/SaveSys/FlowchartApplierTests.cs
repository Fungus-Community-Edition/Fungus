using Amanita.SaveSys;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;

namespace Amanita.SaveSystemTests
{
    public class FlowchartApplierTests : CommonTestFunctionality
    {

        [Test]
        public virtual async Task AppliesVarStates()
        {
            Debug.Log("At start of AppliesVarStates");
            await CommonSetupAsync();
            Debug.Log("Done awaiting common setup async");

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
            await applyTask;

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

        [Test]
        public async Task Apply_WarnsAndSkips_WhenFlowchartIsMissing()
        {
            await CommonSetupAsync();

            // Cache these before destroying anything
            string expectedId = flowchartSaveData.UniqueId;
            string expectedName = flowchartSaveData.FlowchartName;

            RemoveAllFlowchartsFromTheScene();

            // Try to apply save data for a flowchart that no longer exists
            // Should not throw, should log a warning
            LogAssert.Expect(LogType.Warning, $"Flowchart with ID {flowchartSaveData.UniqueId} or name {flowchartSaveData.FlowchartName} not found.");
            await flowchartApplier.Apply(flowchartSaveData);


        }

        protected virtual void RemoveAllFlowchartsFromTheScene()
        {
            IList<Flowchart> toRemove = null;

#if UNITY_6000_0_OR_NEWER
            toRemove = Object.FindObjectsByType<Flowchart>(FindObjectsSortMode.None);
#else
            toRemove = Object.FindObjectsOfType<Flowchart>();
#endif
            foreach (var fc in toRemove)
            {
                // We want to skip the FCs that are part of the AmanitaManager prefab, since that's
                // too core to the functionality of Amanita itself
                bool isPartOfMainManager = fc.GetComponentInParent<AmanitaManager>() != null;
                if (isPartOfMainManager)
                {
                    continue;
                }
                Object.DestroyImmediate(fc.gameObject);
            }
        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenVariableIsMissing()
        {
            await CommonSetupAsync();

            // Remove a variable from the flowchart
            var removedVar = flowchart.Variables.FirstOrDefault();
            Assume.That(removedVar != null, "Test scene must have at least one variable.");
            flowchart.Variables.Remove(removedVar);

            // SaveData still refers to the removed variable
            // Should not throw, should log a warning for the missing variable
            LogAssert.Expect(LogType.Warning, $"Variable {removedVar.Key} not found in flowchart {flowchart.name}.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

        [Test]
        public async Task Apply_WarnsAndSkips_WhenBlockIsMissing()
        {
            await CommonSetupAsync();

            // Remove a block from the flowchart
            var removedBlock = flowchart.GetComponents<Block>().FirstOrDefault();
            Assume.That(removedBlock != null, "Test scene must have at least one block.");
            Object.DestroyImmediate(removedBlock);

            // SaveData still refers to the removed block
            // Should not throw, should log a warning for the missing block
            LogAssert.Expect(LogType.Warning, $"Block {removedBlock.BlockName} not found in flowchart {flowchart.name}.");
            await flowchartApplier.Apply(flowchartSaveData);
        }

    }
}